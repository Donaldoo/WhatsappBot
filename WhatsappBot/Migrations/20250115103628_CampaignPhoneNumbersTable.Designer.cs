﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WhatsappBot.Data;

#nullable disable

namespace WhatsappBot.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250115103628_CampaignPhoneNumbersTable")]
    partial class CampaignPhoneNumbersTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WhatsappBot.Models.Campaign", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("InitialMessage")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Campaigns");
                });

            modelBuilder.Entity("WhatsappBot.Models.CampaignPhoneNumbers", b =>
                {
                    b.Property<Guid>("PhoneNumberId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CampaignId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("PhoneNumberId", "CampaignId");

                    b.HasIndex("CampaignId");

                    b.ToTable("CampaignPhoneNumbers");
                });

            modelBuilder.Entity("WhatsappBot.Models.PhoneNumbers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Company")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("PhoneNumbers");
                });

            modelBuilder.Entity("WhatsappBot.Models.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Image")
                        .HasColumnType("text");

                    b.Property<string>("Link")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .HasColumnType("text");

                    b.Property<string>("PriceNotFormatted")
                        .HasColumnType("text");

                    b.Property<int>("StockValue")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("WhatsappBot.Models.CampaignPhoneNumbers", b =>
                {
                    b.HasOne("WhatsappBot.Models.Campaign", "Campaign")
                        .WithMany("CampaignNumbersCollection")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatsappBot.Models.PhoneNumbers", "PhoneNumber")
                        .WithMany("CampaignNumbersCollection")
                        .HasForeignKey("PhoneNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Campaign");

                    b.Navigation("PhoneNumber");
                });

            modelBuilder.Entity("WhatsappBot.Models.Campaign", b =>
                {
                    b.Navigation("CampaignNumbersCollection");
                });

            modelBuilder.Entity("WhatsappBot.Models.PhoneNumbers", b =>
                {
                    b.Navigation("CampaignNumbersCollection");
                });
#pragma warning restore 612, 618
        }
    }
}
