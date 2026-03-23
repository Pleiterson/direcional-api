using Direcional.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Apartamento> Apartamentos => Set<Apartamento>();
        public DbSet<Reserva> Reservas => Set<Reserva>();
        public DbSet<Venda> Vendas => Set<Venda>();
        public DbSet<Corretor> Corretores => Set<Corretor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
                entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
                entity.Property(x => x.Telefone).HasColumnName("telefone").HasMaxLength(20);
                entity.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("datetime2");

                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Apartamento>(entity =>
            {
                entity.ToTable("apartamentos");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.Numero).HasColumnName("numero").HasMaxLength(10).IsRequired();
                entity.Property(x => x.Andar).HasColumnName("andar").IsRequired();
                entity.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("datetime2");
            });

            modelBuilder.Entity<Corretor>(entity =>
            {
                entity.ToTable("corretores");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
                entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
                entity.Property(x => x.SenhaHash).HasColumnName("senha_hash").HasMaxLength(255).IsRequired();
                entity.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
                entity.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("datetime2");

                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.ToTable("reservas");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
                entity.Property(x => x.ApartamentoId).HasColumnName("apartamento_id").IsRequired();
                entity.Property(x => x.CorretorId).HasColumnName("corretor_id");
                entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(x => x.DataReserva).HasColumnName("data_reserva").HasColumnType("datetime2");

                entity.HasOne(x => x.Cliente)
                    .WithMany(x => x.Reservas)
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Apartamento)
                    .WithMany(x => x.Reservas)
                    .HasForeignKey(x => x.ApartamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Corretor)
                    .WithMany(x => x.Reservas)
                    .HasForeignKey(x => x.CorretorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Venda>(entity =>
            {
                entity.ToTable("vendas");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
                entity.Property(x => x.ApartamentoId).HasColumnName("apartamento_id").IsRequired();
                entity.Property(x => x.ReservaId).HasColumnName("reserva_id");
                entity.Property(x => x.CorretorId).HasColumnName("corretor_id");
                entity.Property(x => x.ValorFinal).HasColumnName("valor_final").HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(x => x.DataVenda).HasColumnName("data_venda").HasColumnType("datetime2");

                entity.HasOne(x => x.Cliente)
                    .WithMany(x => x.Vendas)
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Apartamento)
                    .WithMany(x => x.Vendas)
                    .HasForeignKey(x => x.ApartamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Reserva)
                    .WithMany()
                    .HasForeignKey(x => x.ReservaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.Corretor)
                    .WithMany(x => x.Vendas)
                    .HasForeignKey(x => x.CorretorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
