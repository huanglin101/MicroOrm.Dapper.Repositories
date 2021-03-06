﻿using System;
using System.Collections.Generic;
using System.Linq;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.Tests.Classes;
using Xunit;

namespace MicroOrm.Dapper.Repositories.Tests.Tests
{
    public class SqlGeneratorTests
    {
        [Fact]
        public void ChangeDate_Insert()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);

            var user = new User {Name = "Dude"};
            userSqlGenerator.GetInsert(user);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public void ChangeDate_Update()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);

            var user = new User {Name = "Dude"};
            userSqlGenerator.GetUpdate(user);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public void ContainsExpression()
        {
            var list = new List<int>
            {
                6, 12
            };

            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);

            var isExceptions = false;

            try
            {
                userSqlGenerator.GetSelectAll(x => list.Contains(x.Id));
            }
            catch (NotImplementedException ex)
            {
                Assert.Contains(ex.Message, "predicate can't parse");
                isExceptions = true;
            }

            Assert.True(isExceptions, "Contains no cast exception");
        }

        [Fact]
        public void MSSQLIsNull()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(user => user.UpdatedAt == null);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] " +
                         "WHERE [Users].[UpdatedAt] IS NULL AND [Users].[Deleted] != 1", sqlQuery.GetSql());
            Assert.DoesNotContain("== NULL", sqlQuery.GetSql());
        }

        [Fact]
        public void MSSQLJoinBracelets()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(null, user => user.Cars);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[Deleted], [Users].[UpdatedAt], " +
                         "[Cars].[Id], [Cars].[Name], [Cars].[Data], [Cars].[UserId], [Cars].[Status] " +
                         "FROM [Users] LEFT JOIN [Cars] ON [Users].[Id] = [Cars].[UserId] " +
                         "WHERE [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public void MSSQLJoinOnJoin()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(null, address => address.Users, address => address.Users.Select(user => user.Cars));

            Assert.Equal("SELECT [Addresses].[Id], [Addresses].[Street], " +
                         "[Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[Deleted], [Users].[UpdatedAt], " +
                         "[Cars].[Id], [Cars].[Name], [Cars].[Data], [Cars].[UserId], [Cars].[Status] " +
                         "FROM [Addresses] LEFT JOIN [Users] ON [Addresses].[Id] = [Users].[AddressId] " +
                         "LEFT JOIN [Cars] ON [Users].[Id] = [Cars].[UserId]", sqlQuery.GetSql());
        }

        [Fact]
        public void MSSQLSelectBetweenWithLogicalDeleteBraclets()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] " +
                         "WHERE [Users].[Deleted] != 1 AND [Users].[Id] BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

		[Fact]
		public void MSSQLSelectBetweenWithLogicalDelete()
		{
			ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, false);
			var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

			Assert.Equal("SELECT Users.Id, Users.Name, Users.AddressId, Users.Deleted, Users.UpdatedAt FROM Users " +
						 "WHERE Users.Deleted != 1 AND Users.Id BETWEEN '1' AND '10'", sqlQuery.GetSql());
		}

		[Fact]
        public void MSSQLSelectBetweenWithoutLogicalDeleteBraclets()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

			Assert.Equal("SELECT [Addresses].[Id], [Addresses].[Street] FROM [Addresses] " +
						  "WHERE [Addresses].[Id] BETWEEN '1' AND '10'", sqlQuery.GetSql());
		}


		[Fact]
		public void MSSQLSelectBetweenWithoutLogicalDelete()
		{
			ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(ESqlConnector.MSSQL, false);
			var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

			Assert.Equal("SELECT Addresses.Id, Addresses.Street FROM Addresses " +
						 "WHERE Addresses.Id BETWEEN '1' AND '10'", sqlQuery.GetSql());
		}

		[Fact]
        public void MSSQLSelectFirst()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MSSQL, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.Id == 2);
            Assert.Equal(sqlQuery.GetSql(), "SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] WHERE [Users].[Id] = @Id AND [Users].[Deleted] != 1");
        }

        [Fact]
        public void MySQLIsNotNull()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MySQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(user => user.UpdatedAt != null);

            Assert.Equal("SELECT `Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt` FROM `Users` " +
                         "WHERE `Users`.`UpdatedAt` IS NOT NULL AND `Users`.`Deleted` != 1", sqlQuery.GetSql());
            Assert.DoesNotContain("!= NULL", sqlQuery.GetSql());
        }

        [Fact]
        public void MySQLIsNotNullAND()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MySQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(user => user.Name == "Frank" && user.UpdatedAt != null);

            Assert.Equal("SELECT `Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt` FROM `Users` " +
                         "WHERE `Users`.`Name` = @Name AND `Users`.`UpdatedAt` IS NOT NULL AND `Users`.`Deleted` != 1", sqlQuery.GetSql());
            Assert.DoesNotContain("!= NULL", sqlQuery.GetSql());

            sqlQuery = userSqlGenerator.GetSelectAll(user => user.UpdatedAt != null && user.Name == "Frank");
            Assert.Equal("SELECT `Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt` FROM `Users` " +
                         "WHERE `Users`.`UpdatedAt` IS NOT NULL AND `Users`.`Name` = @Name AND `Users`.`Deleted` != 1", sqlQuery.GetSql());
            Assert.DoesNotContain("!= NULL", sqlQuery.GetSql());
        }

        [Fact]
        public void MySQLJoinOnJoin()
        {
            ISqlGenerator<Car> userSqlGenerator = new SqlGenerator<Car>(ESqlConnector.MySQL, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(null, car => car.User,
                car => car.User.Address);

            Assert.Equal("SELECT `Cars`.`Id`, `Cars`.`Name`, `Cars`.`Data`, `Cars`.`UserId`, `Cars`.`Status`, " +
                         "`Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt`, " +
                         "`Addresses`.`Id`, `Addresses`.`Street` " +
                         "FROM `Cars` LEFT JOIN `Users` ON `Cars`.`UserId` = `Users`.`Id` LEFT JOIN `Addresses` ON `Users`.`AddressId` = `Addresses`.`Id` " +
                         "WHERE `Cars`.`Status` != -1", sqlQuery.GetSql());
        }

        [Fact]
        public void MySQLSelectBetween()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MySQL, true);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT `Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt` FROM `Users` " +
                         "WHERE `Users`.`Deleted` != 1 AND `Users`.`Id` BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

        [Fact]
        public void MySQLSelectFirst()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(ESqlConnector.MySQL, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.Id == 6);
            Assert.Equal(sqlQuery.GetSql(), "SELECT `Users`.`Id`, `Users`.`Name`, `Users`.`AddressId`, `Users`.`Deleted`, `Users`.`UpdatedAt` FROM `Users` WHERE `Users`.`Id` = @Id AND `Users`.`Deleted` != 1 LIMIT 1");
        }
    }
}