using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DbUp.Oracle;
using Moq;
using Xunit;

namespace DbUp.Oracle.Tests
{
    public class OracleTableJournalTests
    {
        [Fact]
        public async Task DeleteOldJournalEntriesAsync_DeletesEntriesOlderThanSpecifiedDate()
        {
            // Arrange
            var mockDbCommand = new Mock<System.Data.Common.DbCommand>();
            var mockParameterCollection = new Mock<System.Data.Common.DbParameterCollection>();
            var mockParameter = new Mock<System.Data.Common.DbParameter>();

            mockDbCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);
            mockDbCommand.SetupGet(c => c.Parameters).Returns(mockParameterCollection.Object);
            mockDbCommand.SetupSet(c => c.CommandText = It.IsAny<string>());
            mockDbCommand.SetupSet(c => c.CommandType = CommandType.Text);
            mockDbCommand
                .Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            Func<IDbCommand> dbCommandFactory = () => mockDbCommand.Object;

            var journal = new OracleTableJournal(
                () => null,
                () => null,
                "myschema",
                "mytable"
            );

            var olderThan = new DateTime(2023, 1, 1);

            // Act
            await journal.DeleteOldJournalEntriesAsync(dbCommandFactory, olderThan);

            // Assert
            mockDbCommand.VerifySet(c => c.CommandText = It.Is<string>(s => s.Contains("delete from")), Times.Once);
            mockDbCommand.VerifySet(c => c.CommandType = CommandType.Text, Times.Once);
            mockDbCommand.Verify(c => c.CreateParameter(), Times.Once);
            mockParameter.VerifySet(p => p.ParameterName = "olderThan", Times.Once);
            mockParameter.VerifySet(p => p.Value = olderThan, Times.Once);
            mockParameterCollection.Verify(c => c.Add(mockParameter.Object), Times.Once);
            mockDbCommand.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOldJournalEntriesAsync_ThrowsIfCommandIsNotDbCommand()
        {
            // Arrange
            var mockDbCommand = new Mock<IDbCommand>();
            Func<IDbCommand> dbCommandFactory = () => mockDbCommand.Object;

            var journal = new OracleTableJournal(
                () => null,
                () => null,
                "myschema",
                "mytable"
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await journal.DeleteOldJournalEntriesAsync(dbCommandFactory, DateTime.UtcNow);
            });
        }
    }
}