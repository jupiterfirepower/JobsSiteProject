﻿// <auto-generated />
using System;
using Jobs.VacancyApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Jobs.VacancyApi.Migrations
{
    [DbContext(typeof(JobsDbContext))]
    [Migration("20241009151147_InitialDB")]
    partial class InitialDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Jobs.Entities.Models.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("CategoryId");

                    b.HasIndex("ParentId");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            CategoryId = 1,
                            CategoryName = "Категорії вакансій",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6624),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6625)
                        },
                        new
                        {
                            CategoryId = 2,
                            CategoryName = "IT, комп''ютери, інтернет",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6662),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6662),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 3,
                            CategoryName = "Адмiнiстрацiя, керівництво середньої ланки",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6663),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6664),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 4,
                            CategoryName = "Будівництво, архітектура",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6665),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6665),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 5,
                            CategoryName = "Бухгалтерія, аудит",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6666),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6666),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 6,
                            CategoryName = "Готельно-ресторанний бізнес, туризм",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6667),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6667),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 7,
                            CategoryName = "Дизайн, творчість",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6668),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6668),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 8,
                            CategoryName = "ЗМІ, видавництво, поліграфія",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6669),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6669),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 9,
                            CategoryName = "Краса, фітнес, спорт",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6670),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6670),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 10,
                            CategoryName = "Культура, музика, шоу-бізнес",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6671),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6671),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 11,
                            CategoryName = "Логістика, склад, ЗЕД",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 12,
                            CategoryName = "Маркетинг, реклама, PR",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6672),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6673),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 13,
                            CategoryName = "Медицина, фармацевтика",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6673),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6674),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 14,
                            CategoryName = "Нерухомість",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 15,
                            CategoryName = "Освіта, наука",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6675),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6676),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 16,
                            CategoryName = "Охорона, безпека",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6676),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6677),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 17,
                            CategoryName = "Продаж, закупівля",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6677),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6678),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 18,
                            CategoryName = "Робочі спеціальності, виробництво",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6678),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6679),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 19,
                            CategoryName = "Роздрібна торгівля",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6679),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6680),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 20,
                            CategoryName = "Секретаріат, діловодство, АГВ",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6680),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6681),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 21,
                            CategoryName = "Сільське господарство, агробізнес",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6681),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6682),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 22,
                            CategoryName = "Страхування",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6682),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6683),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 23,
                            CategoryName = "Сфера обслуговування",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6683),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 24,
                            CategoryName = "Телекомунікації та зв'язок",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6684),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 25,
                            CategoryName = "Топменеджмент, керівництво вищої ланки",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6685),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6685),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 26,
                            CategoryName = "Транспорт, автобізнес",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6686),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6686),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 27,
                            CategoryName = "Управління персоналом",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6687),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6687),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 28,
                            CategoryName = "Фінанси, банк",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6688),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6688),
                            ParentId = 1
                        },
                        new
                        {
                            CategoryId = 29,
                            CategoryName = "Юриспруденція",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6689),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6689),
                            ParentId = 1
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.Company", b =>
                {
                    b.Property<int>("CompanyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CompanyId"));

                    b.Property<string>("CompanyDescription")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<string>("CompanyLink")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("CompanyLogoPath")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("CompanyId");

                    b.ToTable("Companies");

                    b.HasData(
                        new
                        {
                            CompanyId = 1,
                            CompanyDescription = "Test Company Description",
                            CompanyLink = "/company/link",
                            CompanyLogoPath = "/company/logo.png",
                            CompanyName = "Test Company",
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6757),
                            IsActive = true,
                            IsVisible = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6754)
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.CompanyOwnerEmails", b =>
                {
                    b.Property<int>("CompanyOwnerEmailsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CompanyOwnerEmailsId"));

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("CompanyOwnerEmailsId");

                    b.HasIndex("CompanyId");

                    b.ToTable("CompanyOwnerEmails");

                    b.HasData(
                        new
                        {
                            CompanyOwnerEmailsId = 1,
                            CompanyId = 1,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 972, DateTimeKind.Utc).AddTicks(7898),
                            IsActive = true,
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 972, DateTimeKind.Utc).AddTicks(7895),
                            UserEmail = "jupiterfiretetraedr@gmail.com"
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.EmploymentType", b =>
                {
                    b.Property<int>("EmploymentTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("EmploymentTypeId"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EmploymentTypeName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("EmploymentTypeId");

                    b.ToTable("EmploymentTypes");

                    b.HasData(
                        new
                        {
                            EmploymentTypeId = 1,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6807),
                            EmploymentTypeName = "full-time",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6806)
                        },
                        new
                        {
                            EmploymentTypeId = 2,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6809),
                            EmploymentTypeName = "part-time",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6808)
                        },
                        new
                        {
                            EmploymentTypeId = 3,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6810),
                            EmploymentTypeName = "temporary",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6809)
                        },
                        new
                        {
                            EmploymentTypeId = 4,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6811),
                            EmploymentTypeName = "contract",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6811)
                        },
                        new
                        {
                            EmploymentTypeId = 5,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6812),
                            EmploymentTypeName = "freelance",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6812)
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.SecretApiKey", b =>
                {
                    b.Property<int>("KeyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("KeyId"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("KeyId");

                    b.ToTable("ApiKeys");

                    b.HasData(
                        new
                        {
                            KeyId = 1,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6734),
                            IsActive = true,
                            Key = "123456789123456789123",
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6729)
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.Vacancy", b =>
                {
                    b.Property<int>("VacancyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("VacancyId"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double?>("SalaryFrom")
                        .HasColumnType("double precision");

                    b.Property<double?>("SalaryTo")
                        .HasColumnType("double precision");

                    b.Property<string>("VacancyDescription")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("character varying(10000)");

                    b.Property<string>("VacancyTitle")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("VacancyId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CompanyId");

                    b.ToTable("Vacancies");
                });

            modelBuilder.Entity("Jobs.Entities.Models.VacancyEmploymentTypes", b =>
                {
                    b.Property<int>("VacancyId")
                        .HasColumnType("integer");

                    b.Property<int>("EmploymentTypeId")
                        .HasColumnType("integer");

                    b.HasKey("VacancyId", "EmploymentTypeId");

                    b.ToTable("VacancyEmploymentTypes");
                });

            modelBuilder.Entity("Jobs.Entities.Models.VacancyWorkTypes", b =>
                {
                    b.Property<int>("VacancyId")
                        .HasColumnType("integer");

                    b.Property<int>("WorkTypeId")
                        .HasColumnType("integer");

                    b.HasKey("VacancyId", "WorkTypeId");

                    b.ToTable("VacancyWorkTypes");
                });

            modelBuilder.Entity("Jobs.Entities.Models.WorkType", b =>
                {
                    b.Property<int>("WorkTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("WorkTypeId"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("WorkTypeName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("WorkTypeId");

                    b.ToTable("WorkTypes");

                    b.HasData(
                        new
                        {
                            WorkTypeId = 1,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6780),
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6778),
                            WorkTypeName = "Office"
                        },
                        new
                        {
                            WorkTypeId = 2,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6782),
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6781),
                            WorkTypeName = "Remote"
                        },
                        new
                        {
                            WorkTypeId = 3,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6783),
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6782),
                            WorkTypeName = "Office/Remote"
                        },
                        new
                        {
                            WorkTypeId = 4,
                            Created = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6784),
                            Modified = new DateTime(2024, 10, 9, 15, 11, 46, 970, DateTimeKind.Utc).AddTicks(6784),
                            WorkTypeName = "Hybrid"
                        });
                });

            modelBuilder.Entity("Jobs.Entities.Models.Category", b =>
                {
                    b.HasOne("Jobs.Entities.Models.Category", "ParentCategory")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("Jobs.Entities.Models.CompanyOwnerEmails", b =>
                {
                    b.HasOne("Jobs.Entities.Models.Company", "CurrentCompany")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentCompany");
                });

            modelBuilder.Entity("Jobs.Entities.Models.Vacancy", b =>
                {
                    b.HasOne("Jobs.Entities.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Jobs.Entities.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Company");
                });
#pragma warning restore 612, 618
        }
    }
}