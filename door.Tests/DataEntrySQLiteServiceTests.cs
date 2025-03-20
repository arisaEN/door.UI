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
        /// �R���X�g���N�^�Ńf�[�^��p��
        /// </summary>
        public DataEntryServiceTests()
        {
            bool useMock = false; // Moq �Ńe�X�g����ꍇ�� true, SQLite �̏ꍇ�� false

            if (useMock)
            {
                // Moq �� Mock �T�[�r�X���쐬
                var mockService = new Mock<IDataEntrySQLiteService>();

                var mockData = new List<DataEntryDTO>
                {
                    new DataEntryDTO { Date = "2025-03-15", Time = "12:00", StatusName = "�J" },
                    new DataEntryDTO { Date = "2025-03-15", Time = "12:05", StatusName = "��" }
                };

                // GetDataEntryAsync �̖߂�l��ݒ�
                mockService.Setup(service => service.GetDataEntryAsync()).ReturnsAsync(mockData);

                // �f�[�^�}���̃e�X�g�p���b�N
                mockService.Setup(service => service.DataEntryInsertAsync(It.IsAny<DataEntryRequestDto>()))
                    .Returns(Task.CompletedTask);

                _service = mockService.Object;
            }
            else
            {
                // SQLite �� InMemory DB ���쐬
                _dbContext = GetDbContext();
                _service = new DataEntrySQLiteService(_dbContext);
            }
        }

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

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetDataEntryAsync�Ő������f�[�^�擾�ł��邩()
        {
            // Act
            var result = await _service.GetDataEntryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task DataEntryInsertAsync�Ő������f�[�^��}���ł��邩()
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
                Assert.Equal(3, result.Count); // �V�����G���g�����ǉ�����Ă��邱�Ƃ��m�F
            }
        }
    }
}
