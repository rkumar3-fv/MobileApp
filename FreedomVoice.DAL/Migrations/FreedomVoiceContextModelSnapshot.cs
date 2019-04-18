﻿// <auto-generated />
using System;
using FreedomVoice.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FreedomVoice.DAL.Migrations
{
    [DbContext(typeof(FreedomVoiceContext))]
    partial class FreedomVoiceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity("FreedomVoice.DAL.DbEntities.Conversation", b =>
                {
                    b.Property<long>("Id");

                    b.Property<long?>("CollocutorPhoneId");

                    b.Property<long?>("CurrentPhoneId");

                    b.Property<DateTime>("LastSyncDate");

                    b.HasKey("Id");

                    b.HasIndex("CollocutorPhoneId");

                    b.HasIndex("CurrentPhoneId");

                    b.ToTable("Conversation");
                });

            modelBuilder.Entity("FreedomVoice.DAL.DbEntities.Message", b =>
                {
                    b.Property<long>("Id");

                    b.Property<long>("ConversationId");

                    b.Property<long?>("FromId");

                    b.Property<DateTime?>("ReadAt");

                    b.Property<DateTime?>("ReceivedAt");

                    b.Property<DateTime?>("SentAt");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<long?>("ToId");

                    b.HasKey("Id");

                    b.HasIndex("ConversationId");

                    b.HasIndex("FromId");

                    b.HasIndex("ToId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("FreedomVoice.DAL.DbEntities.Phone", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("PhoneNumber")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Phone");
                });

            modelBuilder.Entity("FreedomVoice.DAL.DbEntities.Conversation", b =>
                {
                    b.HasOne("FreedomVoice.DAL.DbEntities.Phone", "CollocutorPhone")
                        .WithMany()
                        .HasForeignKey("CollocutorPhoneId");

                    b.HasOne("FreedomVoice.DAL.DbEntities.Phone", "CurrentPhone")
                        .WithMany()
                        .HasForeignKey("CurrentPhoneId");
                });

            modelBuilder.Entity("FreedomVoice.DAL.DbEntities.Message", b =>
                {
                    b.HasOne("FreedomVoice.DAL.DbEntities.Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("FreedomVoice.DAL.DbEntities.Phone", "From")
                        .WithMany()
                        .HasForeignKey("FromId");

                    b.HasOne("FreedomVoice.DAL.DbEntities.Phone", "To")
                        .WithMany()
                        .HasForeignKey("ToId");
                });
#pragma warning restore 612, 618
        }
    }
}
