using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationAPP.Models;

public partial class YampiBarbershopContext : DbContext
{
    public YampiBarbershopContext()
    {
    }

    public YampiBarbershopContext(DbContextOptions<YampiBarbershopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atencion> Atencions { get; set; }

    public virtual DbSet<BitacoraCliente> BitacoraClientes { get; set; }

    public virtual DbSet<BitacoraPago> BitacoraPagos { get; set; }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Privilegio> Privilegios { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwCitasDelDium> VwCitasDelDia { get; set; }

    public virtual DbSet<VwClientesFrecuente> VwClientesFrecuentes { get; set; }

    public virtual DbSet<VwEstadoAtencion> VwEstadoAtencions { get; set; }

    public virtual DbSet<VwPagosRealizado> VwPagosRealizados { get; set; }

    public virtual DbSet<VwRolesPrivilegio> VwRolesPrivilegios { get; set; }

    public virtual DbSet<VwUsuariosRole> VwUsuariosRoles { get; set; }

    

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Atencion>(entity =>
        {
            entity.HasKey(e => e.IdAtencion).HasName("PK__Atencion__D0A40236964961F9");

            entity.ToTable("Atencion");

            entity.Property(e => e.IdAtencion).HasColumnName("id_atencion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Atencions)
                .HasForeignKey(d => d.IdCita)
                .HasConstraintName("FK_Atencion_Citas");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Atencions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Atencion_Clientes");
        });

        modelBuilder.Entity<BitacoraCliente>(entity =>
        {
            entity.HasKey(e => e.IdBitacora).HasName("PK__Bitacora__7E4268B06328B701");

            entity.Property(e => e.IdBitacora).HasColumnName("id_bitacora");
            entity.Property(e => e.Accion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
        });

        modelBuilder.Entity<BitacoraPago>(entity =>
        {
            entity.HasKey(e => e.IdBitacora).HasName("PK__Bitacora__7E4268B04BE8D54F");

            entity.Property(e => e.IdBitacora).HasColumnName("id_bitacora");
            entity.Property(e => e.Accion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
        });

        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.IdCita).HasName("PK__Citas__6AEC3C09FAA5DDFC");

            entity.ToTable(tb => tb.HasTrigger("trg_EvitarCitaDuplicada"));

            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.Barbero)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("barbero");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Hora).HasColumnName("hora");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Citas_Clientes");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Clientes__677F38F5C6D7EA4B");

            entity.ToTable(tb => tb.HasTrigger("trg_BitacoraClientes"));

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__Pagos__0941B07420F56711");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_ActualizarReporte");
                    tb.HasTrigger("trg_BitacoraPagos");
                    tb.HasTrigger("trg_ValidarMontoPago");
                });

            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Metodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metodo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Clientes");
        });

        modelBuilder.Entity<Privilegio>(entity =>
        {
            entity.HasKey(e => e.IdPrivilegio).HasName("PK__Privileg__7CE03A1CC5AB2111");

            entity.Property(e => e.IdPrivilegio).HasColumnName("id_privilegio");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PK__Reportes__87E4F5CBDF3BECA3");

            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IngresosTotales)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ingresos_totales");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol)
                .HasName("PK__Roles__6ABCB5E0200EB176");

            entity.ToTable("Roles");

            entity.Property(e => e.IdRol)
                .HasColumnName("id_rol");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descripcion");

            entity.Property(e => e.Estado)
                .HasColumnName("estado");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasMany(d => d.IdPrivilegios)
                .WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolesPrivilegio",

                    r => r.HasOne<Privilegio>()
                          .WithMany()
                          .HasForeignKey("IdPrivilegio")
                          .OnDelete(DeleteBehavior.ClientSetNull)
                          .HasConstraintName("FK_RolesPrivilegios_Privilegios"),

                    l => l.HasOne<Role>()
                          .WithMany()
                          .HasForeignKey("IdRol")
                          .OnDelete(DeleteBehavior.ClientSetNull)
                          .HasConstraintName("FK_RolesPrivilegios_Roles"),

                    j =>
                    {
                        j.HasKey("IdRol", "IdPrivilegio")
                         .HasName("PK__RolesPri__BD72B641FCABBDE1");

                        j.ToTable("RolesPrivilegios");

                        j.IndexerProperty<int>("IdRol")
                         .HasColumnName("id_rol");

                        j.IndexerProperty<int>("IdPrivilegio")
                         .HasColumnName("id_privilegio");
                    });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD8BBE2F68");

            entity.HasIndex(e => e.Username, "UQ__Usuarios__F3DBC572E9920D8E").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("correoElectronico");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<VwCitasDelDium>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CitasDelDia");

            entity.Property(e => e.Barbero)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("barbero");
            entity.Property(e => e.Cliente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Hora).HasColumnName("hora");
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
        });

        modelBuilder.Entity<VwClientesFrecuente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ClientesFrecuentes");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<VwEstadoAtencion>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_EstadoAtencion");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<VwPagosRealizado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PagosRealizados");

            entity.Property(e => e.Cliente)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Metodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metodo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
        });

        modelBuilder.Entity<VwRolesPrivilegio>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_RolesPrivilegios");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Privilegio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwUsuariosRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UsuariosRoles");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
