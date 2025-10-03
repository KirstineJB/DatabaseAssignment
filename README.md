Overblik:

Der er blevet gjort brug af .Net console app med Entity Framework Core til at skabe første iteration af et schema,
hvorefter ændringer er foregået via feature branches.
Ved hver ændring er der først blevet brugt en change-based approach. EF migration er blevet brugt, hvorefter databasen er opdateret.
Ved state-based approach er der blevet genereres der roll-up idempotente scripts, der kan køres flere gange uden at ødelægge noget.

alle artefakter og migrations filer er checket in på Git med tagged releases. Artifacter kan findes i mappen db-scripts. 
Projektet hedder: DatabaseAssignment/StudentManagement

Tech Stack:

* .Net SDK (console app)
* EF Core packages: Microsoft.EntityFrameworkCore
* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.EntityFrameworkCore.Design
* dotnet tool install --global dotnet-ef

Struktur af projekt:

StudentManagement/
Models/ -  classes
Migrations/ - change-based migration files (checked in)
artifacts/ - SQL artifacts (change-based og state-based)
SchoolContext.cs  DbContext (mapping only)
SchoolContextFactory.cs - Design-time + runtime factory med connection string
Program.cs - Applies migrations on startup
README.md

Change‑based Migrations (Generelt workflow per ændring)

1. Ny feature branch, "feature/ChangeBased/"featureNavn""
2. Opdater model og mapping (I models mappe og SchoolContext.OnModelCreating)
3. Tilføj migration via terminal: dotnet ef migrations add "Navn"
4. Kør opdatering via terminal: otnet ef database update
5. Lav et artifact: dotnet ef migrations script <Name> -o db_scripts/ChangeBased_"Name".sql

State-based (Generelt workflow per ændring)

I en state-based tilgang beskriver man, hvordan databasen skal se ud i sin endelige tilstand. Deployment-værktøjet sammenligner den nuværende database med modellen og genererer de nødvendige ændringer. Der er gjort brug af: dotnet ef migrations script 0 --idempotent -o artifacts/StateBased_vNext.sql
genereres ét samlet idempotent script, der kan bringe databasen fra enhver tilstand op til nyeste version.
0 angiver startmigrationen, her en tom database uden noget ændringer. Det gøres for at generere et script der indeholder alle migrationer fra begyndelsen og frem til den seneste version.

Man kan argumentere for at gøre brug af Ef idempotent script ikke er fuldkommen state-agtigt da det stabig bygger på ændringshistorikken.

En anden måde at gøre det på er f.eks:
* lav den ønskede endelige tilstand i et Database Project, og lad SSDT generere et diff-script.

* Opret et SQL Server Database Project og importer den aktuelle database.

* Brug Refactor → Rename på kolonner for at udløse sp_rename.

* Byg for at producere en .dacpac.

Generér et deployment-script med Publish eller sqlpackage (gem som artifacts/StateBased_v"NæsteVersion".sql).

Fordele: matcher state-based workflows.
Ulempe: uden en log kan nogle renames potentielt blive til drop/add.

Implementeret ændringer:

1) Initial baseline
Tables: Students, Courses, Enrollments with FKs (Enrollments.StudentId, Enrollments.CourseId).
Migration: InitialCreate.
Formål: etabler baseline

2) Add Student.MiddleName (optional)

Model: public string? MiddleName { get; set; }.

Tanker: Valgfri, kan være null da ikke alle har mellemnavne. Gør det nemmere at implementere i en database med data i.

Migration: AddStudentMiddleName.

3) Add Student.DateOfBirth (nullable)

Model: public DateTime? DateOfBirth { get; set; } (nullable for at gøre opgradering nemmere.).

Mapping: .HasColumnType("date") (optional).

Tanker: Non‑destructive; intet backfill nødvendig. 

Migration: AddStudentDateOfBirth.

4) AddInstructorAndCourse F

Model: Instructor entity; in Course: int? InstructorId, Instructor? Instructor.

Mapping:

modelBuilder.Entity<Course>()
  .HasOne(c => c.Instructor)
  .WithMany(i => i.Courses)
  .HasForeignKey(c => c.InstructorId)
  .OnDelete(DeleteBehavior.SetNull);

Migration: AddInstructorAndCourse

5) Rename Enrollments.Grade → FinalGrade

Migration edit: Replace drop/add med RenameColumn:

migrationBuilder.RenameColumn(
  name: "Grade",
  table: "Enrollments",
  newName: "FinalGrade");

Tanker: Preserves data, non‑destructive, reversible.

Migration: RenameEnrollmentGradeToFinalGrade.

6) Add Department with DepartmentHeadId → Instructor

Model:

Department(Id, Name, Budget, StartDate, int? DepartmentHeadId, Instructor? DepartmentHead)

Optional inverse nav on Instructor: ICollection<Department> DepartmentsHeaded

Mapping:

modelBuilder.Entity<Department>(e =>
{
  e.Property(d => d.Name).IsRequired().HasMaxLength(100);
  e.Property(d => d.Budget).HasColumnType("decimal(18,2)");
  e.Property(d => d.StartDate).HasColumnType("date");


  e.HasOne(d => d.DepartmentHead)
   .WithMany(i => i.DepartmentsHeaded)
   .HasForeignKey(d => d.DepartmentHeadId)
   .OnDelete(DeleteBehavior.SetNull);


  e.HasIndex(d => d.DepartmentHeadId); // non-unique (many departments can share a head)
});

Rationale: FK til instructor(Id) sørger for at kun instruktører kan være departmenthead. Der er ingen unik constraint så flere departments kan have samme hoved. Non destructive.

Migration: AddDepartmentAndHeadRelationship.

7) Ændrer Course.Credits type )

int → decimal(5,2)

Model: public decimal Credits { get; set; }

Mapping: .HasPrecision(5,2) 

Migration: AlterColumn<decimal>(type: "decimal(5,2)") (non‑destructive, ændrer: 5 → 5.00)

Rollback:

Liste af migrationer: dotnet ef migrations list

Rul DB tilbage: dotnet ef database update <TidligereMigration> eller 0 for en hel tom db.

Fjern sidstemigration: dotnet ef migrations remove


Design valg: Destructive vs Non-destructive

* Renames - Grade to Final grade. Valgte non-destructive i dette tilfælde. Det gøres for at bibeholde data. Hvis man gjorde brug af destructive (drop/add) ville man enten miste data eller skulle bruge tid på backfill.
  
* Nye kolonner - Jeg valgte at starte med at gøre dem nullable. Det gør jeg for ikke at ødelægge eksisterende rows. Senere med backfill data kan det ændres til not null hvis det ønskes.
  
* FKs Nullable - med deleteBehavior.SetNull for at lave sikre slet. Et eksempel er at en afdeling ikke bliver slettet selvom department chef bliver fjernet.
  
* type ændring - gør brug af alter (non destructive)

  Man kan argumentere for at der ikke var et behov for at gøre brug af non-disruptive i dette scenarie da databasen ikke var gået live, så det er ikke nødvendigvis en database hvor det at man mister data er et problem. 
