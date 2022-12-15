﻿// <auto-generated />
using System;
using GlobalBan.WebCore.Server.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GlobalBan.WebCore.Server.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    partial class SqliteDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFInfraction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdminGuid")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AdminUserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Evidence")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("InfractionGuid")
                        .HasColumnType("TEXT");

                    b.Property<int>("InfractionScope")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InfractionStatus")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InfractionType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ServerId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("Submitted")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.ToTable("Infractions");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFInstance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Active")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ApiKey")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("InstanceGuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("InstanceIp")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("InstanceName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastConnected")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfileGame")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfileGuid")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfileIdentifier")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Reputation")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFProfileMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("Changed")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ProfileMetas");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFInfraction", b =>
                {
                    b.HasOne("GlobalBan.WebCore.Server.Models.EFInstance", "Instance")
                        .WithMany()
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GlobalBan.WebCore.Server.Models.EFProfile", "Profile")
                        .WithMany("Infractions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instance");

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFProfileMeta", b =>
                {
                    b.HasOne("GlobalBan.WebCore.Server.Models.EFProfile", "Profile")
                        .WithMany("Identity")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("GlobalBan.WebCore.Server.Models.EFProfile", b =>
                {
                    b.Navigation("Identity");

                    b.Navigation("Infractions");
                });
#pragma warning restore 612, 618
        }
    }
}
