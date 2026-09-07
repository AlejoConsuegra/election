using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Election.CRM.API.Features.NombreZona;
using Election.CRM.API.Features.Ocasionale;
using Election.CRM.API.Features.Registro;
using Election.CRM.API.Features.Role;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.Votante;
using Election.CRM.API.Features.AsignacionZona;

namespace Election.CRM.API.Data;

public partial class CrmContext : DbContext
{
    public CrmContext()
    {
    }

    public CrmContext(DbContextOptions<CrmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsignacionZona> AsignacionZonas { get; set; }
    public virtual DbSet<NombreZona> NombreZonas { get; set; }
    public virtual DbSet<Ocasionale> Ocasionales { get; set; }
    public virtual DbSet<Registro> Registros { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Votante> Votantes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AsignacionZona>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("asignacion_zonas");

            entity.HasIndex(e => e.IdUser, "asignacion_zonas_ibfk_2");
            entity.HasIndex(e => e.IdZona, "id_zona");

            entity.HasIndex(e => new { e.IdUser, e.IdZona })
                .IsUnique()
                .HasDatabaseName("uk_asignacion_zona_user");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); // ✅ Nuevo nombre
            entity.Property(e => e.IdUser)
                .HasColumnType("int(11)")
                .HasColumnName("id_user"); // ✅ FK mantiene patrón
            entity.Property(e => e.IdZona)
                .HasColumnType("int(11)")
                .HasColumnName("id_zona"); // ✅ FK mantiene patrón

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.AsignacionZonas)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asignacion_zonas_ibfk_2");

            entity.HasOne(d => d.IdZonaNavigation).WithMany(p => p.AsignacionZonas)
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asignacion_zonas_ibfk_1");
        });

        modelBuilder.Entity<NombreZona>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("nombre_zonas");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.NameZona)
                .HasMaxLength(255)
                .HasColumnName("name_zona");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); // ✅ Nuevo campo
        });

        modelBuilder.Entity<Ocasionale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("ocasionales");

            entity.HasIndex(e => new { e.Cedula, e.IdZona })
                .IsUnique()
                .HasDatabaseName("uk_ocasional_cedula_zona");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.Cedula)
                .HasMaxLength(255)
                .HasColumnName("cedula");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); // ✅ Nuevo nombre
            entity.Property(e => e.IdUser)
                .HasColumnType("int(11)")
                .HasColumnName("id_user");
            entity.Property(e => e.IdZona)
                .HasColumnType("int(11)")
                .HasColumnName("id_zona");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Ocasionales)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ocasionales_ibfk_1");

            entity.HasOne(d => d.IdZonaNavigation).WithMany()
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ocasionales_ibfk_2");
        });

        modelBuilder.Entity<Registro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("registros");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.Barrio)
                .HasMaxLength(255)
                .HasColumnName("barrio");
            entity.Property(e => e.Cedula)
                .HasMaxLength(255)
                .HasColumnName("cedula");
            entity.Property(e => e.Celular)
                .HasMaxLength(255)
                .HasColumnName("celular");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .HasColumnName("direccion");
            entity.Property(e => e.IdUser)
                .HasColumnType("int(11)")
                .HasColumnName("id_user");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Puesto)
                .HasMaxLength(255)
                .HasColumnName("puesto");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); // ✅ Nuevo campo

            entity.HasOne(d => d.IdUserNavigation)
                .WithMany(p => p.Registros)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("registros_ibfk_1");

            entity.HasOne(d => d.Votante)
                .WithOne(p => p.IdRegistroNavigation)
                .HasForeignKey<Votante>(d => d.IdRegistro)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("votantes_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("roles");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.NameRol)
                .HasMaxLength(255)
                .HasColumnName("name_rol");

            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("users");

            entity.HasIndex(e => e.IdRol, "id_rol");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id"); // ✅ PK ahora es "id"
            entity.Property(e => e.Cedula)
                .HasMaxLength(255)
                .HasColumnName("cedula");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IdRol)
                .HasColumnType("int(11)")
                .HasColumnName("id_rol");
            entity.Property(e => e.Municipio)
                .HasMaxLength(255)
                .HasColumnName("municipio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Telefono)
                .HasMaxLength(255)
                .HasColumnName("telefono");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); // ✅ Nuevo campo

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_ibfk_2");
        });

        modelBuilder.Entity<Votante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("votantes");

            entity.HasIndex(e => e.IdRegistro, "uk_votantes_id_registro")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at"); 
            entity.Property(e => e.IdRegistro)
                .HasColumnType("int(11)")
                .HasColumnName("id_registro");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");

            /* entity.HasOne(d => d.IdRegistroNavigation)
                .WithOne(p => p.Votante)
                .HasForeignKey<Votante>(d => d.IdRegistro)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("votantes_ibfk_1"); */
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}