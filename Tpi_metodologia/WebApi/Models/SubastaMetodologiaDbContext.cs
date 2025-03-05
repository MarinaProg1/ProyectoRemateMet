using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models;

public partial class SubastaMetodologiaDbContext : DbContext
{
    public SubastaMetodologiaDbContext()
    {
    }

    public SubastaMetodologiaDbContext(DbContextOptions<SubastaMetodologiaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CuentasBancaria> CuentasBancarias { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Oferta> Ofertas { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Remate> Remates { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

  //  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //    => optionsBuilder.UseSqlServer("Data Source=MARINA_LOPEZ;Initial Catalog=SubastaMetodologiaDB;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentasBancaria>(entity =>
        {
            entity.HasKey(e => e.IdCuenta).HasName("PK__Cuentas___C7E286853638250C");

            entity.ToTable("Cuentas_Bancarias");

            entity.HasIndex(e => e.NumeroCuenta, "UQ__Cuentas___C6B74B88F2C6A3DD").IsUnique();

            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreBanco)
                .HasMaxLength(100)
                .HasColumnName("nombre_banco");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(50)
                .HasColumnName("numero_cuenta");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.CuentasBancaria)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Cuentas_B__id_us__3F466844");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PK__Facturas__6C08ED532748D84F");

            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Facturas__id_ofe__52593CB8");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.IdOferta).HasName("PK__Ofertas__2B7BF92FDB35356B");

            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Ofertas__id_prod__4F7CD00D");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Ofertas__id_usua__4E88ABD4");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__FF341C0DCA05E724");

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.IdRemate).HasColumnName("id_remate");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Imagenes).HasColumnName("imagenes");
            entity.Property(e => e.PrecioBase)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_base");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdRemateNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdRemate)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Productos__id_re__48CFD27E");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Productos__id_us__49C3F6B7");
        });

        modelBuilder.Entity<Remate>(entity =>
        {
            entity.HasKey(e => e.IdRemate).HasName("PK__Remates__CB0F103DEC168170");

            entity.Property(e => e.IdRemate).HasColumnName("id_remate");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasColumnName("categoria");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("en_preparacion")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime")
                .HasColumnName("fecha_cierre");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.Ganancia)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ganancia");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Remates)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Remates__id_usua__440B1D61");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD408AB87A");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__AB6E6164E774EB38").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .HasColumnName("apellido");
            entity.Property(e => e.Clave)
                .HasMaxLength(255)
                .HasColumnName("clave");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo")
                .HasColumnName("estado");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValue("user")
                .HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
