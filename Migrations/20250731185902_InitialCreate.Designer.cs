﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Trackify.Api.Data;

#nullable disable

namespace Trackify.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250731185902_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Icon")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Trackify.Api.Models.Content", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Contents");
                });

            modelBuilder.Entity("Trackify.Api.Models.UpdateLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ContentId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ContentId1")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ContentId");

                    b.HasIndex("ContentId1");

                    b.ToTable("Updates");
                });

            modelBuilder.Entity("Trackify.Api.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Trackify.Api.Models.UserPreference", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ContentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId1")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ContentId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId1");

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("Trackify.Api.Models.UserUpdate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsRead")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UpdateLogId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UpdateLogId1")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId1")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UpdateLogId");

                    b.HasIndex("UpdateLogId1");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId1");

                    b.ToTable("UserUpdates");
                });

            modelBuilder.Entity("Trackify.Api.Models.Content", b =>
                {
                    b.HasOne("Category", "Category")
                        .WithMany("Contents")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Trackify.Api.Models.UpdateLog", b =>
                {
                    b.HasOne("Trackify.Api.Models.Content", "Content")
                        .WithMany()
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trackify.Api.Models.Content", null)
                        .WithMany("Updates")
                        .HasForeignKey("ContentId1");

                    b.Navigation("Content");
                });

            modelBuilder.Entity("Trackify.Api.Models.UserPreference", b =>
                {
                    b.HasOne("Trackify.Api.Models.Content", "Content")
                        .WithMany()
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trackify.Api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trackify.Api.Models.User", null)
                        .WithMany("Preferences")
                        .HasForeignKey("UserId1");

                    b.Navigation("Content");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Trackify.Api.Models.UserUpdate", b =>
                {
                    b.HasOne("Trackify.Api.Models.UpdateLog", "UpdateLog")
                        .WithMany()
                        .HasForeignKey("UpdateLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trackify.Api.Models.UpdateLog", null)
                        .WithMany("UserUpdates")
                        .HasForeignKey("UpdateLogId1");

                    b.HasOne("Trackify.Api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trackify.Api.Models.User", null)
                        .WithMany("Updates")
                        .HasForeignKey("UserId1");

                    b.Navigation("UpdateLog");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Category", b =>
                {
                    b.Navigation("Contents");
                });

            modelBuilder.Entity("Trackify.Api.Models.Content", b =>
                {
                    b.Navigation("Updates");
                });

            modelBuilder.Entity("Trackify.Api.Models.UpdateLog", b =>
                {
                    b.Navigation("UserUpdates");
                });

            modelBuilder.Entity("Trackify.Api.Models.User", b =>
                {
                    b.Navigation("Preferences");

                    b.Navigation("Updates");
                });
#pragma warning restore 612, 618
        }
    }
}
