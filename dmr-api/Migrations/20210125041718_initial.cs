using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Abnormals",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ingredient = table.Column<string>(nullable: true),
                    Batch = table.Column<string>(nullable: true),
                    Building = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abnormals", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BPFCHistories",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BPFCEstablishID = table.Column<int>(nullable: false),
                    Action = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    Before = table.Column<string>(nullable: true),
                    After = table.Column<string>(nullable: true),
                    GlueID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    Remark = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BPFCHistories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false),
                    HourlyOutput = table.Column<int>(nullable: false),
                    ParentID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    BPFCEstablishID = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GlueName",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlueName", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GlueTypes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false),
                    Minutes = table.Column<double>(nullable: false),
                    RPM = table.Column<double>(nullable: false),
                    ParentID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlueTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IngredientInfoReports",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    SupplierName = table.Column<string>(nullable: true),
                    ExpiredTime = table.Column<DateTime>(nullable: false),
                    Batch = table.Column<string>(nullable: true),
                    Qty = table.Column<int>(nullable: false),
                    IngredientInfoID = table.Column<int>(nullable: false),
                    Consumption = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    BuildingName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientInfoReports", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IngredientsInfos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientID = table.Column<int>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    SupplierName = table.Column<string>(nullable: true),
                    Batch = table.Column<string>(nullable: true),
                    ExpiredTime = table.Column<DateTime>(nullable: false),
                    Qty = table.Column<int>(nullable: false),
                    Consumption = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    BuildingName = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientsInfos", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kinds",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kinds", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Line",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Line", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Mailings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    Frequency = table.Column<string>(nullable: true),
                    TimeSend = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mailings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ModelNames",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelNames", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Period",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sequence = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Period", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScaleMachines",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineType = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true),
                    BuildingID = table.Column<int>(nullable: false),
                    MachineID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleMachines", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    isShow = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: true),
                    PasswordSalt = table.Column<byte[]>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUser",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(nullable: false),
                    BuildingID = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUser", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BuildingUser_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LunchTime",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingID = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LunchTime", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LunchTime_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    MachineType = table.Column<string>(nullable: true),
                    GlueTypeID = table.Column<int>(nullable: true),
                    MachineCode = table.Column<string>(nullable: true),
                    MinRPM = table.Column<int>(nullable: false),
                    MaxRPM = table.Column<int>(nullable: false),
                    BuildingID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Settings_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Settings_GlueTypes_GlueTypeID",
                        column: x => x.GlueTypeID,
                        principalTable: "GlueTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelNos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    ModelNameID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelNos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ModelNos_ModelNames_ModelNameID",
                        column: x => x.ModelNameID,
                        principalTable: "ModelNames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(nullable: false),
                    RoleID = table.Column<int>(nullable: false),
                    IsLock = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<string>(nullable: true),
                    MaterialNO = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<int>(nullable: false),
                    isShow = table.Column<bool>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    Unit = table.Column<double>(nullable: false),
                    VOC = table.Column<double>(nullable: false),
                    ExpiredTime = table.Column<double>(nullable: false),
                    DaysToExpiration = table.Column<double>(nullable: false),
                    Real = table.Column<double>(nullable: false),
                    CBD = table.Column<double>(nullable: false),
                    ReplacementFrequency = table.Column<double>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    PrepareTime = table.Column<double>(nullable: false),
                    GlueTypeID = table.Column<int>(nullable: true),
                    SupplierID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Ingredients_GlueTypes_GlueTypeID",
                        column: x => x.GlueTypeID,
                        principalTable: "GlueTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingredients_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Supplier",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleNos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    ModelNoID = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleNos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ArticleNos_ModelNos_ModelNoID",
                        column: x => x.ModelNoID,
                        principalTable: "ModelNos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtProcesses",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleNoID = table.Column<int>(nullable: false),
                    ProcessID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtProcesses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ArtProcesses_ArticleNos_ArticleNoID",
                        column: x => x.ArticleNoID,
                        principalTable: "ArticleNos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtProcesses_Processes_ProcessID",
                        column: x => x.ProcessID,
                        principalTable: "Processes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BPFCEstablishes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelNameID = table.Column<int>(nullable: false),
                    ModelNoID = table.Column<int>(nullable: false),
                    ArticleNoID = table.Column<int>(nullable: false),
                    ArtProcessID = table.Column<int>(nullable: false),
                    ApprovalStatus = table.Column<bool>(nullable: false),
                    FinishedStatus = table.Column<bool>(nullable: false),
                    ApprovalBy = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    Season = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    UpdateTime = table.Column<DateTime>(nullable: true),
                    BuildingDate = table.Column<DateTime>(nullable: true),
                    DueDate = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<bool>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: true),
                    DeleteBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BPFCEstablishes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BPFCEstablishes_ArtProcesses_ArtProcessID",
                        column: x => x.ArtProcessID,
                        principalTable: "ArtProcesses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BPFCEstablishes_ArticleNos_ArticleNoID",
                        column: x => x.ArticleNoID,
                        principalTable: "ArticleNos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BPFCEstablishes_ModelNames_ModelNameID",
                        column: x => x.ModelNameID,
                        principalTable: "ModelNames",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BPFCEstablishes_ModelNos_ModelNoID",
                        column: x => x.ModelNoID,
                        principalTable: "ModelNos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Glues",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Consumption = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<string>(nullable: true),
                    isShow = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    MaterialID = table.Column<int>(nullable: true),
                    ExpiredTime = table.Column<int>(nullable: false),
                    KindID = table.Column<int>(nullable: true),
                    PartID = table.Column<int>(nullable: true),
                    CreatedBy = table.Column<int>(nullable: false),
                    BPFCEstablishID = table.Column<int>(nullable: false),
                    GlueNameID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glues", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Glues_BPFCEstablishes_BPFCEstablishID",
                        column: x => x.BPFCEstablishID,
                        principalTable: "BPFCEstablishes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Glues_GlueName_GlueNameID",
                        column: x => x.GlueNameID,
                        principalTable: "GlueName",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Glues_Kinds_KindID",
                        column: x => x.KindID,
                        principalTable: "Kinds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Glues_Materials_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materials",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Glues_Parts_PartID",
                        column: x => x.PartID,
                        principalTable: "Parts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingID = table.Column<int>(nullable: false),
                    BPFCEstablishID = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    BPFCName = table.Column<string>(nullable: true),
                    HourlyOutput = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    StartWorkingTime = table.Column<DateTime>(nullable: false),
                    FinishWorkingTime = table.Column<DateTime>(nullable: false),
                    WorkingHour = table.Column<int>(nullable: false),
                    IsGenarateTodo = table.Column<bool>(nullable: false),
                    IsRefreshTodo = table.Column<bool>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    ModifyTime = table.Column<DateTime>(nullable: false),
                    DeleteBy = table.Column<int>(nullable: false),
                    CreateBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Plans_BPFCEstablishes_BPFCEstablishID",
                        column: x => x.BPFCEstablishID,
                        principalTable: "BPFCEstablishes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlueIngredient",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlueID = table.Column<int>(nullable: false),
                    IngredientID = table.Column<int>(nullable: false),
                    Allow = table.Column<double>(nullable: false),
                    Percentage = table.Column<double>(nullable: false),
                    CreatedDate = table.Column<string>(nullable: true),
                    Position = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlueIngredient", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GlueIngredient_Glues_GlueID",
                        column: x => x.GlueID,
                        principalTable: "Glues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GlueIngredient_Ingredients_IngredientID",
                        column: x => x.IngredientID,
                        principalTable: "Ingredients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MixingInfos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlueID = table.Column<int>(nullable: false),
                    GlueName = table.Column<string>(nullable: true),
                    BuildingID = table.Column<int>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    MixBy = table.Column<int>(nullable: false),
                    ExpiredTime = table.Column<DateTime>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    EstimatedTime = table.Column<DateTime>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    PrintTime = table.Column<DateTime>(nullable: true),
                    EstimatedStartTime = table.Column<DateTime>(nullable: false),
                    EstimatedFinishTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixingInfos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MixingInfos_Glues_GlueID",
                        column: x => x.GlueID,
                        principalTable: "Glues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanDetails",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlueName = table.Column<string>(nullable: true),
                    BPFCName = table.Column<string>(nullable: true),
                    GlueID = table.Column<int>(nullable: false),
                    Supplier = table.Column<string>(nullable: true),
                    Consumption = table.Column<double>(nullable: false),
                    PlanID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlanDetails_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(nullable: false),
                    GlueID = table.Column<int>(nullable: false),
                    PlanID = table.Column<int>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    CreateBy = table.Column<int>(nullable: false),
                    DeleteBy = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: true),
                    ModifyTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Stations_Glues_GlueID",
                        column: x => x.GlueID,
                        principalTable: "Glues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Stations_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToDoList",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false),
                    GlueID = table.Column<int>(nullable: false),
                    GlueNameID = table.Column<int>(nullable: false),
                    BuildingID = table.Column<int>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    BPFCID = table.Column<int>(nullable: false),
                    LineName = table.Column<string>(nullable: true),
                    GlueName = table.Column<string>(nullable: true),
                    Supplier = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    AbnormalStatus = table.Column<bool>(nullable: false),
                    StartMixingTime = table.Column<DateTime>(nullable: true),
                    FinishMixingTime = table.Column<DateTime>(nullable: true),
                    StartStirTime = table.Column<DateTime>(nullable: true),
                    FinishStirTime = table.Column<DateTime>(nullable: true),
                    StartDispatchingTime = table.Column<DateTime>(nullable: true),
                    FinishDispatchingTime = table.Column<DateTime>(nullable: true),
                    PrintTime = table.Column<DateTime>(nullable: true),
                    StandardConsumption = table.Column<double>(nullable: false),
                    MixedConsumption = table.Column<double>(nullable: false),
                    DeliveredConsumption = table.Column<double>(nullable: false),
                    EstimatedStartTime = table.Column<DateTime>(nullable: false),
                    EstimatedFinishTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    DeleteBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDoList", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToDoList_GlueName_GlueNameID",
                        column: x => x.GlueNameID,
                        principalTable: "GlueName",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ToDoList_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dispatches",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationID = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    StandardAmount = table.Column<double>(nullable: false),
                    Unit = table.Column<string>(nullable: true),
                    IsDelete = table.Column<bool>(nullable: false),
                    StartDispatchingTime = table.Column<DateTime>(nullable: true),
                    FinishDispatchingTime = table.Column<DateTime>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    DeliveryTime = table.Column<DateTime>(nullable: true),
                    DeleteTime = table.Column<DateTime>(nullable: true),
                    DeleteBy = table.Column<int>(nullable: false),
                    CreateBy = table.Column<int>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Dispatches_Buildings_LineID",
                        column: x => x.LineID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Dispatches_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MixingInfoDetails",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Batch = table.Column<string>(maxLength: 50, nullable: true),
                    Position = table.Column<string>(maxLength: 2, nullable: true),
                    Amount = table.Column<double>(nullable: false),
                    IngredientID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixingInfoDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MixingInfoDetails_Ingredients_IngredientID",
                        column: x => x.IngredientID,
                        principalTable: "Ingredients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MixingInfoDetails_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stirs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineID = table.Column<int>(nullable: false),
                    GlueName = table.Column<string>(nullable: true),
                    SettingID = table.Column<int>(nullable: true),
                    RPM = table.Column<int>(nullable: false),
                    ActualDuration = table.Column<int>(nullable: false),
                    StandardDuration = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    TotalMinutes = table.Column<double>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    StartScanTime = table.Column<DateTime>(nullable: false),
                    StartStiringTime = table.Column<DateTime>(nullable: false),
                    FinishStiringTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stirs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Stirs_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stirs_Settings_SettingID",
                        column: x => x.SettingID,
                        principalTable: "Settings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleNos_ModelNoID",
                table: "ArticleNos",
                column: "ModelNoID");

            migrationBuilder.CreateIndex(
                name: "IX_ArtProcesses_ArticleNoID",
                table: "ArtProcesses",
                column: "ArticleNoID");

            migrationBuilder.CreateIndex(
                name: "IX_ArtProcesses_ProcessID",
                table: "ArtProcesses",
                column: "ProcessID");

            migrationBuilder.CreateIndex(
                name: "IX_BPFCEstablishes_ArtProcessID",
                table: "BPFCEstablishes",
                column: "ArtProcessID");

            migrationBuilder.CreateIndex(
                name: "IX_BPFCEstablishes_ArticleNoID",
                table: "BPFCEstablishes",
                column: "ArticleNoID");

            migrationBuilder.CreateIndex(
                name: "IX_BPFCEstablishes_ModelNameID",
                table: "BPFCEstablishes",
                column: "ModelNameID");

            migrationBuilder.CreateIndex(
                name: "IX_BPFCEstablishes_ModelNoID",
                table: "BPFCEstablishes",
                column: "ModelNoID");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUser_BuildingID",
                table: "BuildingUser",
                column: "BuildingID");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_LineID",
                table: "Dispatches",
                column: "LineID");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_GlueIngredient_GlueID",
                table: "GlueIngredient",
                column: "GlueID");

            migrationBuilder.CreateIndex(
                name: "IX_GlueIngredient_IngredientID",
                table: "GlueIngredient",
                column: "IngredientID");

            migrationBuilder.CreateIndex(
                name: "IX_Glues_BPFCEstablishID",
                table: "Glues",
                column: "BPFCEstablishID");

            migrationBuilder.CreateIndex(
                name: "IX_Glues_GlueNameID",
                table: "Glues",
                column: "GlueNameID");

            migrationBuilder.CreateIndex(
                name: "IX_Glues_KindID",
                table: "Glues",
                column: "KindID");

            migrationBuilder.CreateIndex(
                name: "IX_Glues_MaterialID",
                table: "Glues",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_Glues_PartID",
                table: "Glues",
                column: "PartID");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_GlueTypeID",
                table: "Ingredients",
                column: "GlueTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_SupplierID",
                table: "Ingredients",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_LunchTime_BuildingID",
                table: "LunchTime",
                column: "BuildingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfoDetails_IngredientID",
                table: "MixingInfoDetails",
                column: "IngredientID");

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfoDetails_MixingInfoID",
                table: "MixingInfoDetails",
                column: "MixingInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfos_GlueID",
                table: "MixingInfos",
                column: "GlueID");

            migrationBuilder.CreateIndex(
                name: "IX_ModelNos_ModelNameID",
                table: "ModelNos",
                column: "ModelNameID");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDetails_PlanID",
                table: "PlanDetails",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_BPFCEstablishID",
                table: "Plans",
                column: "BPFCEstablishID");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_BuildingID",
                table: "Plans",
                column: "BuildingID");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_BuildingID",
                table: "Settings",
                column: "BuildingID");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_GlueTypeID",
                table: "Settings",
                column: "GlueTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_GlueID",
                table: "Stations",
                column: "GlueID");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_PlanID",
                table: "Stations",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Stirs_MixingInfoID",
                table: "Stirs",
                column: "MixingInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_Stirs_SettingID",
                table: "Stirs",
                column: "SettingID");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoList_GlueNameID",
                table: "ToDoList",
                column: "GlueNameID");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoList_PlanID",
                table: "ToDoList",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles",
                column: "RoleID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abnormals");

            migrationBuilder.DropTable(
                name: "BPFCHistories");

            migrationBuilder.DropTable(
                name: "BuildingUser");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Dispatches");

            migrationBuilder.DropTable(
                name: "GlueIngredient");

            migrationBuilder.DropTable(
                name: "IngredientInfoReports");

            migrationBuilder.DropTable(
                name: "IngredientsInfos");

            migrationBuilder.DropTable(
                name: "Line");

            migrationBuilder.DropTable(
                name: "LunchTime");

            migrationBuilder.DropTable(
                name: "Mailings");

            migrationBuilder.DropTable(
                name: "MixingInfoDetails");

            migrationBuilder.DropTable(
                name: "Period");

            migrationBuilder.DropTable(
                name: "PlanDetails");

            migrationBuilder.DropTable(
                name: "ScaleMachines");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Stirs");

            migrationBuilder.DropTable(
                name: "ToDoList");

            migrationBuilder.DropTable(
                name: "UserDetails");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "MixingInfos");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Glues");

            migrationBuilder.DropTable(
                name: "GlueTypes");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "BPFCEstablishes");

            migrationBuilder.DropTable(
                name: "GlueName");

            migrationBuilder.DropTable(
                name: "Kinds");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "ArtProcesses");

            migrationBuilder.DropTable(
                name: "ArticleNos");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "ModelNos");

            migrationBuilder.DropTable(
                name: "ModelNames");
        }
    }
}
