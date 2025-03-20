using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using door.Domain.DTO;
using door.Infrastructure;
using door.Infrastructure.SQLite;
using door.Domain.Repositories;
using door.Domain.Entities;

namespace door.Tests.TestDataEntrySQLiteService
{
    public class DataEntryServiceTests
    {
        private IDataEntrySQLiteService _service;
        private DoorDbContext _dbContext;
        /// <summary>
        /// コンストラクタでデータを用意
        /// </summary>
        public DataEntryServiceTests()
        {
            bool useMock = false; // Moq でテストする場合は true, SQLite の場合は false

            if (useMock)
            {
                // Moq で Mock サービスを作成
                var mockService = new Mock<IDataEntrySQLiteService>();

                var mockData = new List<DataEntryDTO>
                {
                    new DataEntryDTO { Date = "2025-03-15", Time = "12:00", StatusName = "開" },
                    new DataEntryDTO { Date = "2025-03-15", Time = "12:05", StatusName = "閉" }
                };

                // GetDataEntryAsync の戻り値を設定
                mockService.Setup(service => service.GetDataEntryAsync()).ReturnsAsync(mockData);

                // データ挿入のテスト用モック
                mockService.Setup(service => service.DataEntryInsertAsync(It.IsAny<DataEntryRequestDto>()))
                    .Returns(Task.CompletedTask);

                _service = mockService.Object;
            }
            else
            {
                // SQLite の InMemory DB を作成
                _dbContext = GetDbContext();
                _service = new DataEntrySQLiteService(_dbContext);
            }
        }

        private DoorDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<DoorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // テストごとに異なるDBを作成
                .Options;

            var context = new DoorDbContext(options);

            // 初期データを投入
            context.MasterDoorStatuses.Add(new MasterDoorStatus { Id = 1, DoorStatusName = "開" });
            context.MasterDoorStatuses.Add(new MasterDoorStatus { Id = 2, DoorStatusName = "閉" });

            context.DataEntries.Add(DataEntry.CreateForTest(1, "2025-03-15", "12:00", 1));
            context.DataEntries.Add(DataEntry.CreateForTest(2, "2025-03-15", "12:05", 2));

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetDataEntryAsyncで正しくデータ取得できるか()
        {
            // Act
            var result = await _service.GetDataEntryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task DataEntryInsertAsyncで正しくデータを挿入できるか()
        {
            // Arrange
            var newEntry = new DataEntryRequestDto
            {
                Date = "2025-03-15",
                Time = "12:10",
                DoorStatusId = 1
            };

            // Act
            await _service.DataEntryInsertAsync(newEntry);

            if (_dbContext != null)
            {
                var result = await _dbContext.DataEntries.ToListAsync();
                Assert.Equal(3, result.Count); // 新しいエントリが追加されていることを確認
            }
        }
    }
}
