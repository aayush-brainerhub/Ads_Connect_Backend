using System;
using System.Collections.Generic;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AdsConnect.data;

public partial class AdsConnectContext : DbContext
{
    public AdsConnectContext(DbContextOptions<AdsConnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advertiser> Advertisers { get; set; }

    public virtual DbSet<AdvertisingChannel> AdvertisingChannels { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingItem> BookingItems { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CampaignDeliverable> CampaignDeliverables { get; set; }

    public virtual DbSet<CampaignProviderRequest> CampaignProviderRequests { get; set; }

    public virtual DbSet<CampaignRequirement> CampaignRequirements { get; set; }

    public virtual DbSet<CommissionRule> CommissionRules { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<InventoryAvailability> InventoryAvailabilities { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceLine> InvoiceLines { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlatformFee> PlatformFees { get; set; }

    public virtual DbSet<PricingUnit> PricingUnits { get; set; }

    public virtual DbSet<Proposal> Proposals { get; set; }

    public virtual DbSet<ProposalItem> ProposalItems { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<ProviderChannel> ProviderChannels { get; set; }

    public virtual DbSet<ProviderInventory> ProviderInventories { get; set; }

    public virtual DbSet<ProviderLocation> ProviderLocations { get; set; }

    public virtual DbSet<ProviderPricing> ProviderPricings { get; set; }

    public virtual DbSet<ProviderType> ProviderTypes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("btree_gist")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Advertiser>(entity =>
        {
            entity.HasKey(e => e.AdvertiserId).HasName("Advertiser_pkey");

            entity.ToTable("Advertiser");

            entity.HasIndex(e => e.IndustryId, "IX_Advertiser_IndustryId");

            entity.HasIndex(e => e.LocationId, "IX_Advertiser_LocationId");

            entity.HasIndex(e => e.BusinessName, "IX_Advertiser_Name_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.UserId, "UQ_Advertiser_UserId").IsUnique();

            entity.Property(e => e.AdvertiserId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AddressLine).HasMaxLength(500);
            entity.Property(e => e.BusinessName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.DefaultCurrency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.Gstnumber)
                .HasMaxLength(50)
                .HasColumnName("GSTNumber");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.LegalName).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.MonthlyBudgetMax).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyBudgetMin).HasPrecision(18, 2);
            entity.Property(e => e.Website).HasMaxLength(500);

            entity.HasOne(d => d.Industry).WithMany(p => p.Advertisers)
                .HasForeignKey(d => d.IndustryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Advertiser_Industry");

            entity.HasOne(d => d.Location).WithMany(p => p.Advertisers)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Advertiser_Location");

            entity.HasOne(d => d.User).WithOne(p => p.Advertiser)
                .HasForeignKey<Advertiser>(d => d.UserId)
                .HasConstraintName("FK_Advertiser_User");
        });

        modelBuilder.Entity<AdvertisingChannel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("AdvertisingChannel_pkey");

            entity.ToTable("AdvertisingChannel");

            entity.Property(e => e.ChannelId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Category)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Other'::character varying");
            entity.Property(e => e.ChannelName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MetadataSchema)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.SortOrder).HasDefaultValue((short)0);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("AppUser_pkey");

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.IsActive, "IX_AppUser_Active").HasFilter("\"IsActive\"");

            entity.HasIndex(e => e.PhoneNumber, "UX_AppUser_PhoneNumber")
                .IsUnique()
                .HasFilter("(\"PhoneNumber\" IS NOT NULL)");

            entity.Property(e => e.UserId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsPhoneVerified).HasDefaultValue(false);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasComment("Argon2id/bcrypt digest only. Storing plaintext credentials, tokens, API keys or certificates here is prohibited.");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Preferences)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("AuditLog_pkey");

            entity.ToTable("AuditLog");

            entity.HasIndex(e => new { e.Action, e.CreatedDate }, "IX_AuditLog_Action").IsDescending(false, true);

            entity.HasIndex(e => e.CreatedDate, "IX_AuditLog_Created").IsDescending();

            entity.HasIndex(e => new { e.EntityName, e.EntityId, e.CreatedDate }, "IX_AuditLog_Entity").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.UserId, e.CreatedDate }, "IX_AuditLog_User").IsDescending(false, true);

            entity.Property(e => e.AuditLogId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.RequestId).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLog_User");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("Booking_pkey");

            entity.ToTable("Booking");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_Booking_Active").HasFilter("((\"Status\")::text = ANY ((ARRAY['Confirmed'::character varying, 'InProgress'::character varying])::text[]))");

            entity.HasIndex(e => new { e.AdvertiserId, e.Status, e.BookedDate }, "IX_Booking_Advertiser").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.CampaignId, e.Status }, "IX_Booking_Campaign");

            entity.HasIndex(e => e.ProposalId, "IX_Booking_Proposal");

            entity.HasIndex(e => new { e.ProviderId, e.Status, e.BookedDate }, "IX_Booking_Provider").IsDescending(false, false, true);

            entity.HasIndex(e => e.BookingNumber, "UQ_Booking_BookingNumber").IsUnique();

            entity.HasIndex(e => e.ProposalId, "UQ_Booking_Proposal").IsUnique();

            entity.Property(e => e.BookingId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BookedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.BookingNumber)
                .HasMaxLength(50)
                .HasDefaultValueSql("((('BK-'::text || to_char((CURRENT_DATE)::timestamp with time zone, 'YYYY'::text)) || '-'::text) || lpad((nextval('\"BookingNumberSeq\"'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.PlatformFeeAmount).HasPrecision(18, 2);
            entity.Property(e => e.ProviderPayout).HasPrecision(18, 2);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying");
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.Campaign).WithMany(p => p.Bookings)
                .HasPrincipalKey(p => new { p.CampaignId, p.AdvertiserId })
                .HasForeignKey(d => new { d.CampaignId, d.AdvertiserId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Booking_Campaign");

            entity.HasOne(d => d.Proposal).WithMany(p => p.Bookings)
                .HasPrincipalKey(p => new { p.ProposalId, p.ProviderId })
                .HasForeignKey(d => new { d.ProposalId, d.ProviderId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Booking_Proposal");
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(e => e.BookingItemId).HasName("BookingItem_pkey");

            entity.ToTable("BookingItem");

            entity.HasIndex(e => new { e.BookingId, e.SortOrder }, "IX_BookingItem_Booking");

            entity.HasIndex(e => e.InventoryId, "IX_BookingItem_Inventory");

            entity.Property(e => e.BookingItemId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ItemSpecs)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.PricingUnitCode).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.SortOrder).HasDefaultValue((short)0);
            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(\"Quantity\" * \"UnitPrice\")", true);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingItems)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_BookingItem_Booking");

            entity.HasOne(d => d.Inventory).WithMany(p => p.BookingItems)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_BookingItem_Inventory");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("Campaign_pkey");

            entity.ToTable("Campaign");

            entity.HasIndex(e => new { e.AdvertiserId, e.Status, e.CreatedDate }, "IX_Campaign_Advertiser_Status").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_Campaign_Dates").HasFilter("((\"Status\")::text = 'Active'::text)");

            entity.HasIndex(e => e.IndustryId, "IX_Campaign_IndustryId");

            entity.HasIndex(e => e.CampaignName, "IX_Campaign_Name_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Status, "IX_Campaign_Status");

            entity.HasIndex(e => e.TargetAudience, "IX_Campaign_Targeting")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.HasIndex(e => new { e.CampaignId, e.AdvertiserId }, "UQ_Campaign_Id_Advertiser").IsUnique();

            entity.Property(e => e.CampaignId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            entity.Property(e => e.CampaignName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.Objective)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Awareness'::character varying");
            entity.Property(e => e.RequirementsCount).HasDefaultValue(0);
            entity.Property(e => e.ResponsesCount).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.TargetAudience)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.Advertiser).WithMany(p => p.Campaigns)
                .HasForeignKey(d => d.AdvertiserId)
                .HasConstraintName("FK_Campaign_Advertiser");

            entity.HasOne(d => d.Industry).WithMany(p => p.Campaigns)
                .HasForeignKey(d => d.IndustryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Campaign_Industry");

            entity.HasMany(d => d.Locations).WithMany(p => p.Campaigns)
                .UsingEntity<Dictionary<string, object>>(
                    "CampaignLocation",
                    r => r.HasOne<Location>().WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_CampaignLocation_Location"),
                    l => l.HasOne<Campaign>().WithMany()
                        .HasForeignKey("CampaignId")
                        .HasConstraintName("FK_CampaignLocation_Campaign"),
                    j =>
                    {
                        j.HasKey("CampaignId", "LocationId").HasName("CampaignLocation_pkey");
                        j.ToTable("CampaignLocation");
                        j.HasIndex(new[] { "LocationId" }, "IX_CampaignLocation_LocationId");
                    });
        });

        modelBuilder.Entity<CampaignDeliverable>(entity =>
        {
            entity.HasKey(e => e.DeliverableId).HasName("CampaignDeliverable_pkey");

            entity.ToTable("CampaignDeliverable");

            entity.HasIndex(e => new { e.BookingId, e.Status }, "IX_Deliverable_Booking");

            entity.HasIndex(e => e.BookingItemId, "IX_Deliverable_BookingItem");

            entity.HasIndex(e => e.DueDate, "IX_Deliverable_Due").HasFilter("((\"Status\")::text = ANY ((ARRAY['Pending'::character varying, 'RevisionRequested'::character varying])::text[]))");

            entity.HasIndex(e => e.Metrics, "IX_Deliverable_Metrics")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.HasIndex(e => e.ReviewedBy, "IX_Deliverable_Reviewer");

            entity.HasIndex(e => e.SubmittedBy, "IX_Deliverable_SubmittedBy");

            entity.Property(e => e.DeliverableId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.DeliverableType).HasMaxLength(100);
            entity.Property(e => e.Metrics)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Proof)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.ProofUrl).HasMaxLength(1000);
            entity.Property(e => e.RevisionCount).HasDefaultValue((short)0);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Booking).WithMany(p => p.CampaignDeliverables)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Deliverable_Booking");

            entity.HasOne(d => d.BookingItem).WithMany(p => p.CampaignDeliverables)
                .HasForeignKey(d => d.BookingItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Deliverable_BookingItem");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.CampaignDeliverableReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Deliverable_Reviewer");

            entity.HasOne(d => d.SubmittedByNavigation).WithMany(p => p.CampaignDeliverableSubmittedByNavigations)
                .HasForeignKey(d => d.SubmittedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Deliverable_SubmittedBy");
        });

        modelBuilder.Entity<CampaignProviderRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("CampaignProviderRequest_pkey");

            entity.ToTable("CampaignProviderRequest");

            entity.HasIndex(e => e.CampaignId, "IX_Request_Campaign");

            entity.HasIndex(e => e.ExpiryDate, "IX_Request_Expiring").HasFilter("((\"Status\")::text = ANY ((ARRAY['Pending'::character varying, 'Viewed'::character varying])::text[]))");

            entity.HasIndex(e => e.InventoryId, "IX_Request_Inventory");

            entity.HasIndex(e => new { e.ProviderId, e.Status, e.RequestDate }, "IX_Request_Provider_Status").IsDescending(false, false, true);

            entity.HasIndex(e => e.RequestedBy, "IX_Request_RequestedBy");

            entity.HasIndex(e => new { e.CampaignRequirementId, e.Status }, "IX_Request_Requirement");

            entity.HasIndex(e => new { e.RequestId, e.ProviderId }, "UQ_Request_Id_Provider").IsUnique();

            entity.HasIndex(e => new { e.CampaignRequirementId, e.ProviderId }, "UQ_Request_Requirement_Provider").IsUnique();

            entity.Property(e => e.RequestId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RequestDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying");

            entity.HasOne(d => d.Provider).WithMany(p => p.CampaignProviderRequests)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Request_Provider");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.CampaignProviderRequests)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Request_RequestedBy");

            entity.HasOne(d => d.CampaignRequirement).WithMany(p => p.CampaignProviderRequests)
                .HasPrincipalKey(p => new { p.CampaignRequirementId, p.CampaignId })
                .HasForeignKey(d => new { d.CampaignRequirementId, d.CampaignId })
                .HasConstraintName("FK_Request_Requirement");

            entity.HasOne(d => d.ProviderInventory).WithMany(p => p.CampaignProviderRequests)
                .HasPrincipalKey(p => new { p.InventoryId, p.ProviderId })
                .HasForeignKey(d => new { d.InventoryId, d.ProviderId })
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Request_Inventory");
        });

        modelBuilder.Entity<CampaignRequirement>(entity =>
        {
            entity.HasKey(e => e.CampaignRequirementId).HasName("CampaignRequirement_pkey");

            entity.ToTable("CampaignRequirement");

            entity.HasIndex(e => new { e.CampaignId, e.Status }, "IX_Requirement_Campaign");

            entity.HasIndex(e => new { e.ChannelId, e.Status }, "IX_Requirement_Channel");

            entity.HasIndex(e => e.ProviderTypeId, "IX_Requirement_ProviderType");

            entity.HasIndex(e => e.Specs, "IX_Requirement_Specs")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.HasIndex(e => new { e.CampaignRequirementId, e.CampaignId }, "UQ_Requirement_Id_Campaign").IsUnique();

            entity.Property(e => e.CampaignRequirementId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BudgetMax).HasPrecision(18, 2);
            entity.Property(e => e.BudgetMin).HasPrecision(18, 2);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.MaxUnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.ProposalsCount).HasDefaultValue(0);
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("1");
            entity.Property(e => e.RequestsCount).HasDefaultValue(0);
            entity.Property(e => e.Specs)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignRequirements)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("FK_Requirement_Campaign");

            entity.HasOne(d => d.Channel).WithMany(p => p.CampaignRequirements)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Requirement_Channel");

            entity.HasOne(d => d.ProviderType).WithMany(p => p.CampaignRequirements)
                .HasForeignKey(d => d.ProviderTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Requirement_ProviderType");

            entity.HasMany(d => d.Locations).WithMany(p => p.CampaignRequirements)
                .UsingEntity<Dictionary<string, object>>(
                    "CampaignRequirementLocation",
                    r => r.HasOne<Location>().WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_RequirementLocation_Location"),
                    l => l.HasOne<CampaignRequirement>().WithMany()
                        .HasForeignKey("CampaignRequirementId")
                        .HasConstraintName("FK_RequirementLocation_Requirement"),
                    j =>
                    {
                        j.HasKey("CampaignRequirementId", "LocationId").HasName("CampaignRequirementLocation_pkey");
                        j.ToTable("CampaignRequirementLocation");
                        j.HasIndex(new[] { "LocationId" }, "IX_RequirementLocation_LocationId");
                    });
        });

        modelBuilder.Entity<CommissionRule>(entity =>
        {
            entity.HasKey(e => e.CommissionRuleId).HasName("CommissionRule_pkey");

            entity.ToTable("CommissionRule");

            entity.HasIndex(e => e.AdvertiserId, "IX_Commission_Advertiser");

            entity.HasIndex(e => e.ChannelId, "IX_Commission_Channel");

            entity.HasIndex(e => e.CreatedBy, "IX_Commission_CreatedBy");

            entity.HasIndex(e => new { e.Scope, e.Priority }, "IX_Commission_Lookup")
                .IsDescending(false, true)
                .HasFilter("\"IsActive\"");

            entity.HasIndex(e => e.ProviderId, "IX_Commission_Provider");

            entity.HasIndex(e => e.ProviderTypeId, "IX_Commission_ProviderType");

            entity.Property(e => e.CommissionRuleId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.EffectiveFrom).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FixedAmount).HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxFee).HasPrecision(18, 2);
            entity.Property(e => e.MinFee).HasPrecision(18, 2);
            entity.Property(e => e.PercentageRate).HasPrecision(5, 2);
            entity.Property(e => e.Priority).HasDefaultValue((short)0);
            entity.Property(e => e.Scope).HasMaxLength(20);

            entity.HasOne(d => d.Advertiser).WithMany(p => p.CommissionRules)
                .HasForeignKey(d => d.AdvertiserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Commission_Advertiser");

            entity.HasOne(d => d.Channel).WithMany(p => p.CommissionRules)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Commission_Channel");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CommissionRules)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Commission_CreatedBy");

            entity.HasOne(d => d.Provider).WithMany(p => p.CommissionRules)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Commission_Provider");

            entity.HasOne(d => d.ProviderType).WithMany(p => p.CommissionRules)
                .HasForeignKey(d => d.ProviderTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Commission_ProviderType");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("Conversation_pkey");

            entity.ToTable("Conversation");

            entity.HasIndex(e => e.BookingId, "IX_Conversation_Booking");

            entity.HasIndex(e => e.CampaignId, "IX_Conversation_Campaign");

            entity.HasIndex(e => e.LastMessageDate, "IX_Conversation_Recent")
                .IsDescending()
                .HasNullSortOrder(new[] { NullSortOrder.NullsLast });

            entity.HasIndex(e => e.RequestId, "UX_Conversation_Request")
                .IsUnique()
                .HasFilter("(\"RequestId\" IS NOT NULL)");

            entity.Property(e => e.ConversationId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ContextType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Campaign'::character varying");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsAdminMediated).HasDefaultValue(false);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.MessageCount).HasDefaultValue(0);
            entity.Property(e => e.Subject).HasMaxLength(250);

            entity.HasOne(d => d.Booking).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Conversation_Booking");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Conversation_Campaign");

            entity.HasOne(d => d.Request).WithOne(p => p.Conversation)
                .HasForeignKey<Conversation>(d => d.RequestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Conversation_Request");
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => e.ConversationParticipantId).HasName("ConversationParticipant_pkey");

            entity.ToTable("ConversationParticipant");

            entity.HasIndex(e => new { e.UserId, e.ConversationId }, "IX_Participant_User");

            entity.HasIndex(e => new { e.ConversationId, e.UserId }, "UQ_ConversationParticipant").IsUnique();

            entity.Property(e => e.ConversationParticipantId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsMuted).HasDefaultValue(false);
            entity.Property(e => e.JoinedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastReadSeq).HasDefaultValue(0L);
            entity.Property(e => e.ParticipantRole)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Advertiser'::character varying");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationParticipants)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_Participant_Conversation");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationParticipants)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Participant_User");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasKey(e => e.IndustryId).HasName("Industry_pkey");

            entity.ToTable("Industry");

            entity.Property(e => e.IndustryId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IndustryName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<InventoryAvailability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("InventoryAvailability_pkey");

            entity.ToTable("InventoryAvailability");

            entity.HasIndex(e => new { e.InventoryId, e.Period }, "EX_Availability_NoOverlap")
                .HasFilter("((\"Status\")::text = ANY ((ARRAY['Held'::character varying, 'Booked'::character varying, 'Blocked'::character varying])::text[]))")
                .HasMethod("gist");

            entity.HasIndex(e => e.BookingId, "IX_Availability_Booking").HasFilter("(\"BookingId\" IS NOT NULL)");

            entity.HasIndex(e => new { e.InventoryId, e.Period }, "IX_Availability_Inventory").HasMethod("gist");

            entity.Property(e => e.AvailabilityId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Available'::character varying");

            entity.HasOne(d => d.Booking).WithMany(p => p.InventoryAvailabilities)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Availability_Booking");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryAvailabilities)
                .HasForeignKey(d => d.InventoryId)
                .HasConstraintName("FK_Availability_Inventory");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("Invoice_pkey");

            entity.ToTable("Invoice");

            entity.HasIndex(e => new { e.AdvertiserId, e.Status, e.IssuedDate }, "IX_Invoice_Advertiser").IsDescending(false, false, true);

            entity.HasIndex(e => e.BookingId, "IX_Invoice_Booking");

            entity.HasIndex(e => e.DueDate, "IX_Invoice_Overdue").HasFilter("((\"Status\")::text = ANY ((ARRAY['Issued'::character varying, 'PartiallyPaid'::character varying])::text[]))");

            entity.HasIndex(e => new { e.ProviderId, e.Status, e.IssuedDate }, "IX_Invoice_Provider").IsDescending(false, false, true);

            entity.HasIndex(e => e.InvoiceNumber, "UQ_Invoice_InvoiceNumber").IsUnique();

            entity.Property(e => e.InvoiceId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.BillingSnapshot)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.Direction)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PlatformToAdvertiser'::character varying");
            entity.Property(e => e.DocumentUrl).HasMaxLength(1000);
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(100)
                .HasDefaultValueSql("((('INV-'::text || to_char((CURRENT_DATE)::timestamp with time zone, 'YYYY'::text)) || '-'::text) || lpad((nextval('\"InvoiceNumberSeq\"'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(\"Subtotal\" + \"TaxAmount\")", true);

            entity.HasOne(d => d.Advertiser).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.AdvertiserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Invoice_Advertiser");

            entity.HasOne(d => d.Booking).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Invoice_Booking");

            entity.HasOne(d => d.Provider).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Invoice_Provider");
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasKey(e => e.InvoiceLineId).HasName("InvoiceLine_pkey");

            entity.ToTable("InvoiceLine");

            entity.HasIndex(e => new { e.InvoiceId, e.SortOrder }, "IX_InvoiceLine_Invoice");

            entity.Property(e => e.InvoiceLineId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LineTotal)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(\"Quantity\" * \"UnitPrice\")", true);
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("1");
            entity.Property(e => e.SortOrder).HasDefaultValue((short)0);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceLines)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK_InvoiceLine_Invoice");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("Location_pkey");

            entity.ToTable("Location");

            entity.HasIndex(e => e.City, "IX_Location_City_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.Country, e.State, e.City }, "UQ_Location").IsUnique();

            entity.Property(e => e.LocationId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValueSql("'India'::character varying");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.State).HasMaxLength(100);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("Message_pkey");

            entity.ToTable("Message");

            entity.HasIndex(e => new { e.SenderId, e.SentDate }, "IX_Message_Sender").IsDescending(false, true);

            entity.HasIndex(e => new { e.ConversationId, e.MessageSeq }, "IX_Message_Thread").IsDescending(false, true);

            entity.HasIndex(e => e.MessageSeq, "UQ_Message_Seq").IsUnique();

            entity.Property(e => e.MessageId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AttachmentUrl).HasMaxLength(1000);
            entity.Property(e => e.MessageSeq)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn();
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Text'::character varying");
            entity.Property(e => e.Payload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.SentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_Message_Conversation");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Message_Sender");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("Payment_pkey");

            entity.ToTable("Payment");

            entity.HasIndex(e => new { e.BookingId, e.Direction, e.Status }, "IX_Payment_Booking");

            entity.HasIndex(e => e.InvoiceId, "IX_Payment_Invoice");

            entity.HasIndex(e => e.ParentPaymentId, "IX_Payment_Parent");

            entity.HasIndex(e => new { e.PayeeUserId, e.CreatedDate }, "IX_Payment_Payee").IsDescending(false, true);

            entity.HasIndex(e => new { e.PayerUserId, e.CreatedDate }, "IX_Payment_Payer").IsDescending(false, true);

            entity.HasIndex(e => new { e.Status, e.CreatedDate }, "IX_Payment_Pending").HasFilter("((\"Status\")::text = ANY ((ARRAY['Pending'::character varying, 'Authorized'::character varying])::text[]))");

            entity.HasIndex(e => new { e.Gateway, e.TransactionReference }, "UX_Payment_GatewayRef")
                .IsUnique()
                .HasFilter("(\"TransactionReference\" IS NOT NULL)");

            entity.HasIndex(e => e.IdempotencyKey, "UX_Payment_Idempotency")
                .IsUnique()
                .HasFilter("(\"IdempotencyKey\" IS NOT NULL)");

            entity.Property(e => e.PaymentId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.Direction)
                .HasMaxLength(30)
                .HasDefaultValueSql("'AdvertiserToPlatform'::character varying");
            entity.Property(e => e.Gateway).HasMaxLength(50);
            entity.Property(e => e.GatewayPayload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(200);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying");
            entity.Property(e => e.TransactionReference)
                .HasMaxLength(200)
                .HasComment("Payment-gateway transaction id or vault token only. Storing card numbers, CVV, bank credentials, API keys or certificates here is prohibited.");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Payment_Booking");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Payment_Invoice");

            entity.HasOne(d => d.ParentPayment).WithMany(p => p.InverseParentPayment)
                .HasForeignKey(d => d.ParentPaymentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Payment_Parent");

            entity.HasOne(d => d.PayeeUser).WithMany(p => p.PaymentPayeeUsers)
                .HasForeignKey(d => d.PayeeUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Payment_Payee");

            entity.HasOne(d => d.PayerUser).WithMany(p => p.PaymentPayerUsers)
                .HasForeignKey(d => d.PayerUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Payment_Payer");
        });

        modelBuilder.Entity<PlatformFee>(entity =>
        {
            entity.HasKey(e => e.PlatformFeeId).HasName("PlatformFee_pkey");

            entity.ToTable("PlatformFee");

            entity.HasIndex(e => e.BookingId, "IX_PlatformFee_Booking");

            entity.HasIndex(e => e.CommissionRuleId, "IX_PlatformFee_Rule");

            entity.HasIndex(e => new { e.BookingId, e.FeeType }, "UQ_PlatformFee_Booking_Type").IsUnique();

            entity.Property(e => e.PlatformFeeId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.CalculatedAmount).HasPrecision(18, 2);
            entity.Property(e => e.CalculatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.FeeType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Commission'::character varying");
            entity.Property(e => e.FixedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Percentage).HasPrecision(8, 4);
            entity.Property(e => e.RuleSnapshot)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.Booking).WithMany(p => p.PlatformFees)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PlatformFee_Booking");

            entity.HasOne(d => d.CommissionRule).WithMany(p => p.PlatformFees)
                .HasForeignKey(d => d.CommissionRuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_PlatformFee_Rule");
        });

        modelBuilder.Entity<PricingUnit>(entity =>
        {
            entity.HasKey(e => e.PricingUnitId).HasName("PricingUnit_pkey");

            entity.ToTable("PricingUnit");

            entity.Property(e => e.PricingUnitId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsTimeBased).HasDefaultValue(false);
            entity.Property(e => e.UnitCode).HasMaxLength(50);
            entity.Property(e => e.UnitName).HasMaxLength(100);
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.ProposalId).HasName("Proposal_pkey");

            entity.ToTable("Proposal");

            entity.HasIndex(e => e.DecidedBy, "IX_Proposal_DecidedBy");

            entity.HasIndex(e => new { e.ProviderId, e.Status, e.SubmittedDate }, "IX_Proposal_Provider").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.RequestId, e.Version }, "IX_Proposal_Request").IsDescending(false, true);

            entity.HasIndex(e => new { e.ProposalId, e.ProviderId }, "UQ_Proposal_Id_Provider").IsUnique();

            entity.HasIndex(e => new { e.RequestId, e.Version }, "UQ_Proposal_Version").IsUnique();

            entity.HasIndex(e => e.RequestId, "UX_Proposal_Active_Per_Request")
                .IsUnique()
                .HasFilter("((\"Status\")::text = ANY ((ARRAY['Pending'::character varying, 'UnderReview'::character varying, 'Accepted'::character varying])::text[]))");

            entity.Property(e => e.ProposalId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("((\"Subtotal\" - \"DiscountAmount\") + \"TaxAmount\")", true);
            entity.Property(e => e.Version).HasDefaultValue((short)1);

            entity.HasOne(d => d.DecidedByNavigation).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.DecidedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Proposal_DecidedBy");

            entity.HasOne(d => d.CampaignProviderRequest).WithMany(p => p.Proposals)
                .HasPrincipalKey(p => new { p.RequestId, p.ProviderId })
                .HasForeignKey(d => new { d.RequestId, d.ProviderId })
                .HasConstraintName("FK_Proposal_Request");
        });

        modelBuilder.Entity<ProposalItem>(entity =>
        {
            entity.HasKey(e => e.ProposalItemId).HasName("ProposalItem_pkey");

            entity.ToTable("ProposalItem");

            entity.HasIndex(e => e.InventoryId, "IX_ProposalItem_Inventory");

            entity.HasIndex(e => e.PricingUnitId, "IX_ProposalItem_PricingUnit");

            entity.HasIndex(e => new { e.ProposalId, e.SortOrder }, "IX_ProposalItem_Proposal");

            entity.Property(e => e.ProposalItemId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ItemSpecs)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("1");
            entity.Property(e => e.SortOrder).HasDefaultValue((short)0);
            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("(\"Quantity\" * \"UnitPrice\")", true);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Inventory).WithMany(p => p.ProposalItems)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ProposalItem_Inventory");

            entity.HasOne(d => d.PricingUnit).WithMany(p => p.ProposalItems)
                .HasForeignKey(d => d.PricingUnitId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ProposalItem_PricingUnit");

            entity.HasOne(d => d.Proposal).WithMany(p => p.ProposalItems)
                .HasForeignKey(d => d.ProposalId)
                .HasConstraintName("FK_ProposalItem_Proposal");
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("Provider_pkey");

            entity.ToTable("Provider");

            entity.HasIndex(e => e.TotalAudience, "IX_Provider_Discovery_Audience")
                .IsDescending()
                .HasFilter("(\"IsActive\" AND \"AcceptsRequests\")");

            entity.HasIndex(e => new { e.Rating, e.RatingCount }, "IX_Provider_Discovery_Rating")
                .IsDescending()
                .HasFilter("(\"IsActive\" AND \"AcceptsRequests\")");

            entity.HasIndex(e => e.ProviderTypeId, "IX_Provider_Discovery_Type").HasFilter("(\"IsActive\" AND \"AcceptsRequests\")");

            entity.HasIndex(e => e.ProviderName, "IX_Provider_Name_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.PrimaryLocationId, "IX_Provider_PrimaryLocation");

            entity.HasIndex(e => e.ProviderTypeId, "IX_Provider_ProviderTypeId");

            entity.HasIndex(e => e.UserId, "UQ_Provider_UserId").IsUnique();

            entity.Property(e => e.ProviderId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AcceptsRequests).HasDefaultValue(true);
            entity.Property(e => e.BaseCurrency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.CompletedBookings).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.LegalName).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProviderName).HasMaxLength(200);
            entity.Property(e => e.Rating).HasPrecision(3, 2);
            entity.Property(e => e.RatingCount).HasDefaultValue(0);
            entity.Property(e => e.TotalAudience).HasDefaultValue(0L);
            entity.Property(e => e.Website).HasMaxLength(500);

            entity.HasOne(d => d.PrimaryLocation).WithMany(p => p.Providers)
                .HasForeignKey(d => d.PrimaryLocationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Provider_PrimaryLocation");

            entity.HasOne(d => d.ProviderType).WithMany(p => p.Providers)
                .HasForeignKey(d => d.ProviderTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Provider_ProviderType");

            entity.HasOne(d => d.User).WithOne(p => p.Provider)
                .HasForeignKey<Provider>(d => d.UserId)
                .HasConstraintName("FK_Provider_User");
        });

        modelBuilder.Entity<ProviderChannel>(entity =>
        {
            entity.HasKey(e => e.ProviderChannelId).HasName("ProviderChannel_pkey");

            entity.ToTable("ProviderChannel");

            entity.HasIndex(e => new { e.ChannelId, e.AudienceSize }, "IX_ProviderChannel_Channel_Audience")
                .IsDescending(false, true)
                .HasFilter("\"IsActive\"");

            entity.HasIndex(e => e.AudienceMetrics, "IX_ProviderChannel_Metrics")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.HasIndex(e => e.ProviderId, "IX_ProviderChannel_ProviderId");

            entity.HasIndex(e => new { e.ProviderId, e.ChannelId, e.Handle }, "UQ_ProviderChannel").IsUnique();

            entity.HasIndex(e => e.ProviderId, "UX_ProviderChannel_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\"");

            entity.Property(e => e.ProviderChannelId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AudienceMetrics)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EngagementRate).HasPrecision(5, 2);
            entity.Property(e => e.Handle).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.ProfileUrl).HasMaxLength(500);

            entity.HasOne(d => d.Channel).WithMany(p => p.ProviderChannels)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ProviderChannel_Channel");

            entity.HasOne(d => d.Provider).WithOne(p => p.ProviderChannel)
                .HasForeignKey<ProviderChannel>(d => d.ProviderId)
                .HasConstraintName("FK_ProviderChannel_Provider");
        });

        modelBuilder.Entity<ProviderInventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("ProviderInventory_pkey");

            entity.ToTable("ProviderInventory");

            entity.HasIndex(e => e.ChannelId, "IX_Inventory_Channel").HasFilter("((\"Status\")::text = 'Active'::text)");

            entity.HasIndex(e => e.ProviderLocationId, "IX_Inventory_Location");

            entity.HasIndex(e => e.InventoryName, "IX_Inventory_Name_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.ProviderId, e.Status }, "IX_Inventory_Provider");

            entity.HasIndex(e => e.Specs, "IX_Inventory_Specs")
                .HasMethod("gin")
                .HasOperators(new[] { "jsonb_path_ops" });

            entity.HasIndex(e => new { e.ProviderId, e.InventoryCode }, "UQ_Inventory_Code").IsUnique();

            entity.HasIndex(e => new { e.InventoryId, e.ProviderId }, "UQ_Inventory_Id_Provider").IsUnique();

            entity.Property(e => e.InventoryId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.InventoryCode).HasMaxLength(100);
            entity.Property(e => e.InventoryName).HasMaxLength(200);
            entity.Property(e => e.LeadTimeDays).HasDefaultValue((short)0);
            entity.Property(e => e.QuantityTotal).HasDefaultValue(1);
            entity.Property(e => e.Specs)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Draft'::character varying");

            entity.HasOne(d => d.Channel).WithMany(p => p.ProviderInventories)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Inventory_Channel");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderInventories)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK_Inventory_Provider");

            entity.HasOne(d => d.ProviderLocation).WithMany(p => p.ProviderInventories)
                .HasPrincipalKey(p => new { p.ProviderLocationId, p.ProviderId })
                .HasForeignKey(d => new { d.ProviderLocationId, d.ProviderId })
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Inventory_Location");
        });

        modelBuilder.Entity<ProviderLocation>(entity =>
        {
            entity.HasKey(e => e.ProviderLocationId).HasName("ProviderLocation_pkey");

            entity.ToTable("ProviderLocation");

            entity.HasIndex(e => e.LocationId, "IX_ProviderLocation_LocationId");

            entity.HasIndex(e => e.ProviderId, "IX_ProviderLocation_ProviderId");

            entity.HasIndex(e => new { e.ProviderLocationId, e.ProviderId }, "UQ_ProviderLocation_Id_Provider").IsUnique();

            entity.HasIndex(e => e.ProviderId, "UX_ProviderLocation_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\"");

            entity.Property(e => e.ProviderLocationId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AddressLine1).HasMaxLength(250);
            entity.Property(e => e.AddressLine2).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.PostalCode).HasMaxLength(20);

            entity.HasOne(d => d.Location).WithMany(p => p.ProviderLocations)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ProviderLocation_Location");

            entity.HasOne(d => d.Provider).WithOne(p => p.ProviderLocation)
                .HasForeignKey<ProviderLocation>(d => d.ProviderId)
                .HasConstraintName("FK_ProviderLocation_Provider");
        });

        modelBuilder.Entity<ProviderPricing>(entity =>
        {
            entity.HasKey(e => e.PricingId).HasName("ProviderPricing_pkey");

            entity.ToTable("ProviderPricing");

            entity.HasIndex(e => new { e.InventoryId, e.PricingUnitId, e.ValidPeriod }, "EX_Pricing_NoOverlap").HasMethod("gist");

            entity.HasIndex(e => e.InventoryId, "IX_Pricing_Inventory");

            entity.HasIndex(e => e.ProviderId, "IX_Pricing_Provider");

            entity.HasIndex(e => e.PricingUnitId, "IX_Pricing_Unit");

            entity.HasIndex(e => e.UnitPrice, "IX_Pricing_UnitPrice").HasFilter("(\"IsActive\" AND \"IsDefault\")");

            entity.Property(e => e.PricingId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'INR'::bpchar")
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.MaximumQuantity).HasPrecision(18, 2);
            entity.Property(e => e.MinimumQuantity).HasPrecision(18, 2);
            entity.Property(e => e.PriceTiers)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.PricingName).HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.ValidPeriod).HasDefaultValueSql("daterange(CURRENT_DATE, NULL::date, '[)'::text)");

            entity.HasOne(d => d.PricingUnit).WithMany(p => p.ProviderPricings)
                .HasForeignKey(d => d.PricingUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Pricing_Unit");

            entity.HasOne(d => d.ProviderInventory).WithMany(p => p.ProviderPricings)
                .HasPrincipalKey(p => new { p.InventoryId, p.ProviderId })
                .HasForeignKey(d => new { d.InventoryId, d.ProviderId })
                .HasConstraintName("FK_Pricing_Inventory");
        });

        modelBuilder.Entity<ProviderType>(entity =>
        {
            entity.HasKey(e => e.ProviderTypeId).HasName("ProviderType_pkey");

            entity.ToTable("ProviderType");

            entity.Property(e => e.ProviderTypeId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasDefaultValue((short)0);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("Review_pkey");

            entity.ToTable("Review");

            entity.HasIndex(e => new { e.AdvertiserId, e.CreatedDate }, "IX_Review_Advertiser")
                .IsDescending(false, true)
                .HasFilter("\"IsPublished\"");

            entity.HasIndex(e => e.BookingId, "IX_Review_Booking");

            entity.HasIndex(e => new { e.ProviderId, e.CreatedDate }, "IX_Review_Provider")
                .IsDescending(false, true)
                .HasFilter("\"IsPublished\"");

            entity.HasIndex(e => e.ReviewerUserId, "IX_Review_Reviewer");

            entity.HasIndex(e => new { e.BookingId, e.Direction }, "UQ_Review_Booking_Direction").IsUnique();

            entity.Property(e => e.ReviewId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CriteriaRatings)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Direction)
                .HasMaxLength(30)
                .HasDefaultValueSql("'AdvertiserToProvider'::character varying");
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Advertiser).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.AdvertiserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Review_Advertiser");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Review_Booking");

            entity.HasOne(d => d.Provider).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Review_Provider");

            entity.HasOne(d => d.ReviewerUser).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Review_Reviewer");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("UserRole_pkey");

            entity.ToTable("UserRole");

            entity.HasIndex(e => e.AssignedBy, "IX_UserRole_AssignedBy");

            entity.HasIndex(e => e.RoleId, "IX_UserRole_RoleId");

            entity.Property(e => e.AssignedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.UserRoleAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserRole_AssignedBy");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UserRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRole_User");
        });
        modelBuilder.HasSequence("BookingNumberSeq");
        modelBuilder.HasSequence("InvoiceNumberSeq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
