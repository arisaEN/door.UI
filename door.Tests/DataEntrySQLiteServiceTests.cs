using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using door.Domain.DTO;
using door.Domain.Entities;
using door.Infrastructure;
using door.Infrastructure.SQLite;

namespace door.Tests.TestDataEntrySQLiteService
{ 

    public class DataEntrySQLiteServiceTests
    {
        private DoorDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<DoorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // �e�X�g���ƂɈقȂ�DB���쐬
                .Options;

            var context = new DoorDbContext(options);

            // �����f�[�^�𓊓�
            context.MasterDoorStatuses.Add(new MasterDoorStatus { Id = 1, DoorStatusName = "�J" });
            context.MasterDoorStatuses.Add(new MasterDoorStatus { Id = 2, DoorStatusName = "��" });

            context.DataEntries.Add(DataEntry.CreateForTest(1, "2025-03-15", "12:00", 1));
            context.DataEntries.Add(DataEntry.CreateForTest(2, "2025-03-15", "12:05", 2));

            //context.DataEntries.Add(new DataEntry { Id = 1, Date = "2025-03-15", Time = "12:00", DoorStatusId = 1 });
            //context.DataEntries.Add(new DataEntry { Id = 2, Date = "2025-03-15", Time = "12:05", DoorStatusId = 2 });

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetDataEntryAsync_ReturnsCorrectData()
        {
            // Arrange
            var dbContext = GetDbContext();
            var service = new DataEntrySQLiteService(dbContext);

            // Act
            var result = await service.GetDataEntryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("��", result[0].StatusName);
            Assert.Equal("�J", result[1].StatusName);
        }

        [Fact]
        public async Task DataEntryInsertAsync_AddsNewEntry()
        {
            // Arrange
            var dbContext = GetDbContext();
            var service = new DataEntrySQLiteService(dbContext);

            var newEntry = new DataEntryRequestDto
            {
                Date = "2025-03-15",
                Time = "12:10",
                DoorStatusId = 1
            };

            // Act
            await service.DataEntryInsertAsync(newEntry);
            var result = await dbContext.DataEntries.ToListAsync();

            // Assert
            Assert.Equal(3, result.Count); // �V�����G���g�����ǉ�����Ă��邱�Ƃ��m�F
        }
    }
}