﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Riveet.Prerender.Contexts;

namespace Riveet.Prerender.Migrations
{
    [DbContext(typeof(WebsiteContext))]
    [Migration("20200524045118_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Riveet.Prerender.Entities.WebPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Html")
                        .HasColumnType("text");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Url")
                        .HasColumnType("character varying(2000)")
                        .HasMaxLength(2000);

                    b.HasKey("Id");

                    b.HasIndex("Url");

                    b.ToTable("Website");
                });
#pragma warning restore 612, 618
        }
    }
}
