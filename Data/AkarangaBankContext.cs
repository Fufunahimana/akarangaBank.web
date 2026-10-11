using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore; 
using AkarangaBank.Web.Models; 
 
namespace AkarangaBank.Web.Data; 
    public class AkarangaBankContext : IdentityDbContext<ApplicationUser, ApplicationRole, int> { 
        public AkarangaBankContext(DbContextOptions<AkarangaBankContext> options) : base(options) { 
        } 
     
        public DbSet<Agence> Agences { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Compte> Comptes { get; set; } = null!;
        public DbSet<Virement> Virements { get; set; } = null!;
        public DbSet<Operation> Operations { get; set; } = null!;
        public DbSet<Ecriture> Ecritures { get; set; } = null!;
        public DbSet<Pret> Prets { get; set; } = null!;
        public DbSet<EcheancePret> EcheancesPret { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<AlerteAML> AlertesAML { get; set; } = null!;
        public DbSet<SessionUtilisateur> SessionsUtilisateur { get; set; } = null!;
        public DbSet<VerificationKYC> VerificationsKYC { get; set; } = null!;
        public DbSet<Carte> Cartes { get; set; } = null!;
        public DbSet<Parametre> Parametres { get; set; } = null!;
        public DbSet<TypeCompte> TypesCompte { get; set; } = null!;
        public DbSet<ApplicationUser> Utilisateurs { get; set; } = null!;
        public DbSet<VirementInterAgence> VirementsInterAgences => Set<VirementInterAgence>();
        public DbSet<CompensationAgence> CompensationsAgences => Set<CompensationAgence>();
        public DbSet<DeclarationSoupcon> DeclarationsSoupcon => Set<DeclarationSoupcon>();
        public DbSet<SeuilAML> SeuilsAML => Set<SeuilAML>();
        public DbSet<TypePret> TypesPret => Set<TypePret>();
        public DbSet<TransactionCarte> TransactionsCarte => Set<TransactionCarte>();
         
        protected override void OnModelCreating(ModelBuilder modelBuilder) { 
            base.OnModelCreating(modelBuilder); 
     
            // 1. IDENTITY
            modelBuilder.Entity<ApplicationUser>(b => 
            { 
                b.ToTable("Utilisateurs"); 
                b.Property(u => u.NomComplet).HasMaxLength(100).IsRequired(); 
                b.Property(u => u.Role).HasMaxLength(20).IsRequired(); 
                b.Property(u => u.DeuxFA_Secret).HasMaxLength(255); 
     
                b.HasOne(u => u.Agence) 
                 .WithMany() 
                 .HasForeignKey(u => u.AgenceId) 
                 .OnDelete(DeleteBehavior.SetNull);
    
                b.HasIndex(u => u.AgenceId); 
                b.HasIndex(u => u.Role); 
                b.HasIndex(u => u.ClientId); 
            }); 
     
            modelBuilder.Entity<ApplicationRole>(b => b.ToTable("Roles")); 
            modelBuilder.Entity<IdentityUserRole<int>>(b => b.ToTable("UtilisateursRoles")); 
            modelBuilder.Entity<IdentityUserClaim<int>>(b => b.ToTable("UtilisateursClaims")); 
            modelBuilder.Entity<IdentityUserLogin<int>>(b => b.ToTable("UtilisateursLogins")); 
            modelBuilder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RolesClaims")); 
            modelBuilder.Entity<IdentityUserToken<int>>(b => b.ToTable("UtilisateursTokens")); 
     
            // 2. AGENCE 
            modelBuilder.Entity<Agence>(b => 
            { 
                b.ToTable("Agences"); 
                b.HasKey(a => a.Id); 
                b.Property(a => a.Code).HasMaxLength(20).IsRequired(); 
                b.Property(a => a.Nom).HasMaxLength(100).IsRequired(); 
                b.Property(a => a.Ville).HasMaxLength(50).IsRequired(); 
                b.Property(a => a.EstSiege).HasDefaultValue(false); 
                b.Property(a => a.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(a => a.Code).IsUnique(); 
            }); 
     
            // 3. PARAMETRE 
            modelBuilder.Entity<Parametre>(b => 
            { 
                b.ToTable("Parametres"); 
                b.HasKey(p => p.Id); 
                b.Property(p => p.Cle).HasMaxLength(50).IsRequired(); 
                b.Property(p => p.Valeur).HasMaxLength(255).IsRequired(); 
                b.Property(p => p.Description).HasMaxLength(255);  
                b.Property(p => p.DateModification).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(p => new { p.Cle, p.AgenceId }).IsUnique(); 
                b.HasOne(p => p.Agence).WithMany().HasForeignKey(p => p.AgenceId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 4. CLIENT 
            modelBuilder.Entity<Client>(b => 
            { 
                b.ToTable("Clients"); 
                b.HasKey(c => c.Id); 
                b.Property(c => c.NumeroClient).HasMaxLength(20).IsRequired(); 
                b.Property(c => c.Nom).HasMaxLength(100).IsRequired(); 
                b.Property(c => c.Prenom).HasMaxLength(100).IsRequired(); 
                b.Property(c => c.NumeroCNI).HasMaxLength(30).IsRequired(); 
                b.Property(c => c.Telephone).HasMaxLength(20); 
                b.Property(c => c.Email).HasMaxLength(100); 
                b.Property(c => c.Adresse).HasMaxLength(255); 
                b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Actif"); 
                b.Property(c => c.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(c => c.NumeroClient).IsUnique(); 
                b.HasIndex(c => c.NumeroCNI).IsUnique(); 
                b.HasIndex(c => c.AgenceId); 
                b.HasOne(c => c.Agence).WithMany().HasForeignKey(c => c.AgenceId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 5. VERIFICATION KYC 
            modelBuilder.Entity<VerificationKYC>(b => 
            { 
                b.ToTable("VerificationsKYC"); 
                b.HasKey(k => k.Id);  
                b.Property(k => k.TypeDocument).HasMaxLength(50).IsRequired(); 
                b.Property(k => k.NumeroDocument).HasMaxLength(50).IsRequired(); 
                b.Property(k => k.CheminDocument).HasMaxLength(500); 
                b.Property(k => k.Statut).HasMaxLength(20).HasDefaultValue("EnAttente"); 
                b.Property(k => k.Commentaire).HasMaxLength(500); 
                b.HasOne(k => k.Client).WithMany(client => client.VerificationsKYC).HasForeignKey(k => k.ClientId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(k => k.VerifiePar).WithMany().HasForeignKey(k => k.VerifieParId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 6. TYPE DE COMPTE 
            modelBuilder.Entity<TypeCompte>(b => 
            { 
                b.ToTable("TypesCompte"); 
                b.HasKey(t => t.Id); 
                b.Property(t => t.Code).HasMaxLength(20).IsRequired(); 
                b.Property(t => t.Libelle).HasMaxLength(50).IsRequired(); 
                b.Property(t => t.TauxInteret).HasColumnType("decimal(5,2)").HasDefaultValue(0m); 
                b.Property(t => t.SoldeMinimum).HasColumnType("decimal(18,2)").HasDefaultValue(0m); 
                b.Property(t => t.AutoriseDecouvert).HasDefaultValue(false); 
                b.Property(t => t.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(t => t.Code).IsUnique(); 
            }); 
     
            // 7. COMPTE 
            modelBuilder.Entity<Compte>(b => 
            { 
                b.ToTable("Comptes"); 
                b.HasKey(c => c.Id); 
                b.Property(c => c.NumeroCompte).HasMaxLength(30).IsRequired(); 
                b.Property(c => c.Solde).HasColumnType("decimal(18,2)").HasDefaultValue(0m); 
                b.Property(c => c.Devise).HasMaxLength(3).HasDefaultValue("BIF"); 
                b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Actif"); 
                b.Property(c => c.DateOuverture).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.Property(c => c.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(c => c.NumeroCompte).IsUnique(); 
                b.HasIndex(c => c.ClientId); 
                b.HasIndex(c => c.AgenceId); 
                b.HasIndex(c => c.TypeCompteId); 
                b.HasOne(c => c.Client).WithMany(client => client.Comptes).HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(c => c.TypeCompte).WithMany().HasForeignKey(c => c.TypeCompteId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(c => c.Agence).WithMany().HasForeignKey(c => c.AgenceId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            modelBuilder.Entity<TypePret>(b =>
            {
                b.ToTable("TypesPret");
                b.HasKey(t => t.Id);
                b.Property(t => t.Code).HasMaxLength(30).IsRequired();
                b.Property(t => t.Libelle).HasMaxLength(100).IsRequired();
                b.Property(t => t.MontantMin).HasColumnType("decimal(18,2)");
                b.Property(t => t.MontantMax).HasColumnType("decimal(18,2)");
                b.Property(t => t.TauxMin).HasColumnType("decimal(5,2)");
                b.Property(t => t.TauxMax).HasColumnType("decimal(5,2)");
                b.Property(t => t.Actif).HasDefaultValue(true);
                b.Property(t => t.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                b.HasIndex(t => t.Code).IsUnique();
                b.HasData(
                    new TypePret { Id = 1, Code = "PERSONNEL", Libelle = "Prêt personnel", MontantMin = 100_000m, MontantMax = 50_000_000m, DureeMinMois = 3, DureeMaxMois = 60, TauxMin = 10m, TauxMax = 18m, Actif = true, DateCreation = new DateTime(2026, 1, 1) },
                    new TypePret { Id = 2, Code = "HABITAT", Libelle = "Prêt habitat", MontantMin = 1_000_000m, MontantMax = 500_000_000m, DureeMinMois = 12, DureeMaxMois = 240, TauxMin = 8m, TauxMax = 15m, Actif = true, DateCreation = new DateTime(2026, 1, 1) },
                    new TypePret { Id = 3, Code = "URGENCE", Libelle = "Prêt d'urgence", MontantMin = 50_000m, MontantMax = 10_000_000m, DureeMinMois = 1, DureeMaxMois = 24, TauxMin = 12m, TauxMax = 24m, Actif = true, DateCreation = new DateTime(2026, 1, 1) });
            });

            // 8. CARTE 
            modelBuilder.Entity<Carte>(b => 
            { 
                b.ToTable("Cartes"); 
                b.HasKey(c => c.Id); 
                b.Property(c => c.NumeroCarte).HasMaxLength(20).IsRequired(); 
                b.Property(c => c.NumeroCarteMasque).HasMaxLength(25).HasDefaultValue("**** **** **** 0000").IsRequired();
                b.Property(c => c.TypeCarte).HasMaxLength(20).IsRequired(); 
                b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Active"); 
                b.Property(c => c.MotifBlocage).HasMaxLength(100);
                b.Property(c => c.PlafondRetraitJournalier).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(c => c.PlafondPaiementJournalier).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(c => c.PlafondPaiementEnLigneJournalier).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(c => c.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(c => c.NumeroCarte).IsUnique();  
                b.HasIndex(c => c.CompteId); 
                b.HasIndex(c => c.ClientId); 
                b.HasOne(c => c.Compte).WithMany(compte => compte.Cartes).HasForeignKey(c => c.CompteId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(c => c.Client).WithMany().HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Restrict); 
            }); 

            modelBuilder.Entity<TransactionCarte>(b =>
            {
                b.ToTable("TransactionsCarte");
                b.HasKey(t => t.Id);
                b.Property(t => t.TypeTransaction).HasMaxLength(30).IsRequired();
                b.Property(t => t.Montant).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(t => t.Statut).HasMaxLength(20).HasDefaultValue("Refusee");
                b.Property(t => t.MotifRefus).HasMaxLength(255);
                b.Property(t => t.DateTransaction).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                b.HasIndex(t => new { t.CarteId, t.DateTransaction });
                b.HasOne(t => t.Carte).WithMany(c => c.Transactions).HasForeignKey(t => t.CarteId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(t => t.Operation).WithMany().HasForeignKey(t => t.OperationId).OnDelete(DeleteBehavior.SetNull);
            });
     
            // 9. PRET 
            modelBuilder.Entity<Pret>(b => 
            { 
                b.ToTable("Prets"); 
                b.HasKey(p => p.Id); 
                b.Property(p => p.Reference).HasMaxLength(30).IsRequired(); 
                b.Property(p => p.MontantPrincipal).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(p => p.TauxInteret).HasColumnType("decimal(5,2)").IsRequired(); 
                b.Property(p => p.Mensualite).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(p => p.MontantRembourse).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(p => p.SoldeRestant).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(p => p.Statut).HasMaxLength(20).HasDefaultValue("EnAttente"); 
                b.Property(p => p.Commentaire).HasMaxLength(1000);
                b.Property(p => p.MotifRejet).HasMaxLength(500);
                b.Property(p => p.DateDemande).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(p => p.Reference).IsUnique(); 
                b.HasIndex(p => p.ClientId); 
                b.HasIndex(p => p.TypePretId);
                b.HasIndex(p => p.Statut); 
                b.HasOne(p => p.Client).WithMany().HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(p => p.TypePret).WithMany().HasForeignKey(p => p.TypePretId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.CompteDebit).WithMany().HasForeignKey(p => p.CompteDebitId).OnDelete(DeleteBehavior.Restrict);  
                b.HasOne(p => p.ValidePar).WithMany().HasForeignKey(p => p.ValideParId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 10. ECHEANCE PRET 
            modelBuilder.Entity<EcheancePret>(b => 
            { 
                b.ToTable("EcheancesPret"); 
                b.HasKey(e => e.Id); 
                b.Property(e => e.Capital).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(e => e.Interet).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(e => e.MontantTotal).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(e => e.SoldeRestant).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(e => e.Penalite).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                b.Property(e => e.Statut).HasMaxLength(20).HasDefaultValue("EnAttente"); 
                b.HasIndex(e => new { e.PretId, e.NumeroEcheance }).IsUnique(); 
                b.HasIndex(e => e.DateEcheance); 
                b.HasOne(e => e.Pret).WithMany(p => p.Echeances).HasForeignKey(e => e.PretId).OnDelete(DeleteBehavior.Cascade); 
            }); 
     
            // 11. VIREMENT 
            modelBuilder.Entity<Virement>(b => 
            { 
                b.ToTable("Virements"); 
                b.HasKey(v => v.Id); 
                b.Property(v => v.Reference).HasMaxLength(30).IsRequired(); 
                b.Property(v => v.Montant).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(v => v.Devise).HasMaxLength(3).HasDefaultValue("BIF"); 
                b.Property(v => v.Statut).HasMaxLength(20).HasDefaultValue("EnAttente"); 
                b.Property(v => v.Motif).HasMaxLength(255); 
                b.Property(v => v.ControleAML).HasDefaultValue(false); 
                b.Property(v => v.DateVirement).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(v => v.Reference).IsUnique(); 
                b.HasIndex(v => v.CompteSourceId); 
                b.HasIndex(v => v.CompteDestinationId); 
                b.HasIndex(v => v.Statut); 
                b.HasOne(v => v.CompteSource).WithMany().HasForeignKey(v => v.CompteSourceId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(v => v.CompteDestination).WithMany().HasForeignKey(v => v.CompteDestinationId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(v => v.Utilisateur).WithMany().HasForeignKey(v => v.UtilisateurId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 12. OPERATION 
            modelBuilder.Entity<Operation>(b => 
            { 
                b.ToTable("Operations"); 
                b.HasKey(o => o.Id); 
                b.Property(o => o.Reference).HasMaxLength(30).IsRequired(); 
                b.Property(o => o.TypeOperation).HasMaxLength(30).IsRequired(); 
                b.Property(o => o.Montant).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(o => o.Devise).HasMaxLength(3).HasDefaultValue("BIF"); 
                b.Property(o => o.Statut).HasMaxLength(20).HasDefaultValue("EnAttente"); 
                b.Property(o => o.Commentaire).HasMaxLength(255); 
                b.Property(o => o.DateOperation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.Property(o => o.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(o => o.Reference).IsUnique(); 
                b.HasIndex(o => o.CompteId); 
                b.HasIndex(o => o.UtilisateurId);  
                b.HasIndex(o => o.AgenceId); 
                b.HasIndex(o => o.DateOperation); 
                b.HasIndex(o => o.Statut); 
                b.HasOne(o => o.Compte).WithMany().HasForeignKey(o => o.CompteId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(o => o.Utilisateur).WithMany().HasForeignKey(o => o.UtilisateurId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(o => o.Agence).WithMany().HasForeignKey(o => o.AgenceId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(o => o.Virement).WithMany().HasForeignKey(o => o.VirementId).OnDelete(DeleteBehavior.SetNull); 
                b.HasOne(o => o.Pret).WithMany().HasForeignKey(o => o.PretId).OnDelete(DeleteBehavior.SetNull); 
            }); 
     
            // 13. ECRITURE 
            modelBuilder.Entity<Ecriture>(b => 
            { 
                b.ToTable("Ecritures"); 
                b.HasKey(e => e.Id); 
                b.Property(e => e.Sens).HasMaxLength(10).IsRequired(); 
                b.Property(e => e.Montant).HasColumnType("decimal(18,2)").IsRequired(); 
                b.Property(e => e.Devise).HasMaxLength(3).HasDefaultValue("BIF"); 
                b.Property(e => e.DateEcriture).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.Property(e => e.Libelle).HasMaxLength(255); 
                b.Property(e => e.CodeCompteGeneral).HasMaxLength(50);
                b.HasIndex(e => e.OperationId);  
                b.HasIndex(e => e.CompteId); 
                b.HasOne(e => e.Operation).WithMany(o => o.Ecritures).HasForeignKey(e => e.OperationId).OnDelete(DeleteBehavior.Cascade); 
                b.HasOne(e => e.Compte).WithMany(c => c.Ecritures).HasForeignKey(e => e.CompteId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 14. AUDIT LOG 
            modelBuilder.Entity<AuditLog>(b => 
            { 
                b.ToTable("AuditLog"); 
                b.HasKey(a => a.Id); 
                b.Property(a => a.DateEvenement).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.Property(a => a.Action).HasMaxLength(50).IsRequired(); 
                b.Property(a => a.TableConcernee).HasMaxLength(50); 
                b.Property(a => a.ValeurAvant).HasColumnType("longtext"); 
                b.Property(a => a.ValeurApres).HasColumnType("longtext"); 
                b.Property(a => a.AdresseIP).HasMaxLength(45); 
                b.Property(a => a.UserAgent).HasMaxLength(255); 
                b.Property(a => a.Details).HasColumnType("longtext"); 
                b.HasIndex(a => a.DateEvenement); 
                b.HasIndex(a => a.UtilisateurId); 
                b.HasOne(a => a.Utilisateur).WithMany().HasForeignKey(a => a.UtilisateurId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(a => a.Agence).WithMany().HasForeignKey(a => a.AgenceId).OnDelete(DeleteBehavior.Restrict); 
            }); 
     
            // 15. ALERTE AML 
            modelBuilder.Entity<AlerteAML>(b => 
            { 
                b.ToTable("AlertesAML"); 
                b.HasKey(a => a.Id); 
                b.Property(a => a.TypeAlerte).HasMaxLength(50).IsRequired(); 
                b.Property(a => a.Niveau).HasMaxLength(20).IsRequired(); 
                b.Property(a => a.Description).HasMaxLength(500); 
                b.Property(a => a.Statut).HasMaxLength(20).HasDefaultValue("Ouverte"); 
                b.Property(a => a.DateAlerte).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.HasIndex(a => a.Statut); 
                b.HasIndex(a => a.ClientId); 
                b.HasOne(a => a.Client).WithMany().HasForeignKey(a => a.ClientId).OnDelete(DeleteBehavior.Restrict); 
                b.HasOne(a => a.Operation).WithMany().HasForeignKey(a => a.OperationId).OnDelete(DeleteBehavior.SetNull); 
                b.HasOne(a => a.TraitePar).WithMany().HasForeignKey(a => a.TraiteParId).OnDelete(DeleteBehavior.SetNull); 
            });

            modelBuilder.Entity<VirementInterAgence>(b =>
            {
                b.ToTable("VirementsInterAgences");
                b.HasKey(v => v.Id);
                b.Property(v => v.Reference).HasMaxLength(30).IsRequired();
                b.Property(v => v.Montant).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(v => v.Devise).HasMaxLength(3).HasDefaultValue("BIF");
                b.Property(v => v.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");
                b.Property(v => v.MotifRejet).HasMaxLength(500);
                b.HasIndex(v => v.Reference).IsUnique();
                b.HasIndex(v => v.VirementId).IsUnique();
                b.HasIndex(v => v.Statut);
                b.HasOne(v => v.Virement).WithMany().HasForeignKey(v => v.VirementId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(v => v.AgenceSource).WithMany().HasForeignKey(v => v.AgenceSourceId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(v => v.AgenceDestination).WithMany().HasForeignKey(v => v.AgenceDestinationId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(v => v.ValidePar).WithMany().HasForeignKey(v => v.ValideParId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CompensationAgence>(b =>
            {
                b.ToTable("CompensationsAgences");
                b.HasKey(c => c.Id);
                b.Property(c => c.Reference).HasMaxLength(40).IsRequired();
                b.Property(c => c.CodeCompteGeneral).HasMaxLength(50).IsRequired();
                b.Property(c => c.TotalDebits).HasColumnType("decimal(18,2)");
                b.Property(c => c.TotalCredits).HasColumnType("decimal(18,2)");
                b.Property(c => c.Solde).HasColumnType("decimal(18,2)");
                b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("EnCours");
                b.HasIndex(c => new { c.AgenceId, c.DateCompensation }).IsUnique();
                b.HasOne(c => c.Agence).WithMany().HasForeignKey(c => c.AgenceId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.CompteCompensation).WithMany().HasForeignKey(c => c.CompteCompensationId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DeclarationSoupcon>(b =>
            {
                b.ToTable("DeclarationsSoupcon");
                b.HasKey(d => d.Id);
                b.Property(d => d.Reference).HasMaxLength(30).IsRequired();
                b.Property(d => d.TypeDeclaration).HasMaxLength(10).IsRequired();
                b.Property(d => d.Contenu).HasColumnType("longtext").IsRequired();
                b.Property(d => d.MotifSuspicion).HasMaxLength(1000);
                b.Property(d => d.Statut).HasMaxLength(20).HasDefaultValue("Brouillon");
                b.Property(d => d.ReponseCNRF).HasMaxLength(1000);
                b.HasIndex(d => d.Reference).IsUnique();
                b.HasIndex(d => new { d.AlerteId, d.TypeDeclaration }).IsUnique();
                b.HasOne(d => d.Alerte).WithMany().HasForeignKey(d => d.AlerteId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.Client).WithMany().HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.RedigePar).WithMany().HasForeignKey(d => d.RedigeParId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SeuilAML>(b =>
            {
                b.ToTable("SeuilsAML");
                b.HasKey(s => s.Id);
                b.Property(s => s.Code).HasMaxLength(20).IsRequired();
                b.Property(s => s.Libelle).HasMaxLength(100).IsRequired();
                b.Property(s => s.Montant).HasColumnType("decimal(18,2)");
                b.Property(s => s.Devise).HasMaxLength(3).HasDefaultValue("BIF");
                b.Property(s => s.TypeOperation).HasMaxLength(30).HasDefaultValue("Toutes");
                b.HasIndex(s => new { s.Code, s.TypeOperation }).IsUnique();
                b.HasData(
                    new SeuilAML { Id = 1, Code = "VIGILANCE", Libelle = "Vigilance renforcée", Montant = 5_000_000m, Devise = "BIF", TypeOperation = "Toutes", Actif = true, DateModification = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new SeuilAML { Id = 2, Code = "DECLARATION", Libelle = "Déclaration espèces CNRF", Montant = 10_000_000m, Devise = "BIF", TypeOperation = "Depot", Actif = true, DateModification = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new SeuilAML { Id = 3, Code = "DECLARATION", Libelle = "Déclaration espèces CNRF", Montant = 10_000_000m, Devise = "BIF", TypeOperation = "Retrait", Actif = true, DateModification = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new SeuilAML { Id = 4, Code = "BLOCAGE", Libelle = "Blocage automatique", Montant = 50_000_000m, Devise = "BIF", TypeOperation = "Toutes", Actif = true, DateModification = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
            });
    
            // 16. SESSION UTILISATEUR 
            modelBuilder.Entity<SessionUtilisateur>(b => 
            { 
                b.ToTable("SessionsUtilisateur"); 
                b.HasKey(s => s.Id); 
                b.Property(s => s.Token).HasMaxLength(255).IsRequired(); 
                b.Property(s => s.AdresseIP).HasMaxLength(45); 
                b.Property(s => s.UserAgent).HasMaxLength(255); 
                b.Property(s => s.DeuxFA_Valide).HasDefaultValue(false); 
                b.Property(s => s.DateConnexion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); 
                b.Property(s => s.EstActive).HasDefaultValue(true); 
                b.HasIndex(s => s.Token).IsUnique(); 
                b.HasIndex(s => s.UtilisateurId);  
                b.HasOne(s => s.Utilisateur).WithMany().HasForeignKey(s => s.UtilisateurId).OnDelete(DeleteBehavior.Cascade); 
            }); 
        } 
    }
