using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Core;
using FluentAssertions;
using PromoCodeFactory.DataAccess.Data;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests : IDisposable
    {
        Mock<IRepository<Partner>> _repositoryMock = new Mock<IRepository<Partner>>();
        PartnersController _controller;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _repositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _controller = fixture.Build<PartnersController>().OmitAutoProperties().Create();

            //controller = new PartnersController(_repositoryMock.Object);
        }

        #region Создание объектов для Arrange

        public static Partner GetDefaultPartner(
            bool isActive = true,
            int numberIssuedPromoCodes = 0,
            DateTime? cancelDateOfPartnerPromoCodeLimit = null)
        {
            // Поскольку данные для инициализации БД при проверке записи данных в неё берутся из FakeDataFactory, то и партнёра по умолчанию берём оттуда же.
            Partner partner = FakeDataFactory.Partners[0];
            // А потом параметрами меняем при необходимости.
            partner.IsActive = isActive;
            partner.NumberIssuedPromoCodes = numberIssuedPromoCodes;
            foreach (PartnerPromoCodeLimit limit in partner.PartnerLimits)
                limit.CancelDate = cancelDateOfPartnerPromoCodeLimit;
            return partner;
        }

        public static SetPartnerPromoCodeLimitRequest GetDefaultPartnerPromoCodeLimitRequest(
            DateTime? endDate = null,
            int limit = 100)
        {
            return new SetPartnerPromoCodeLimitRequest
            {
                EndDate = endDate ?? DateTime.Now.AddMonths(1),
                Limit = limit
            };
        }

        public static IEnumerable<object[]> GetBadRequestData()
        {
            return new List<object[]>
            {
                // PartnerIsNotActive
                new object[] { GetDefaultPartner(isActive: false), GetDefaultPartnerPromoCodeLimitRequest() },
                // LimitValueLessThenZero
                new object[] { GetDefaultPartner(), GetDefaultPartnerPromoCodeLimitRequest(limit: -1) },
                
                #region Для тестирования добавленных проверок
                // RequestIsEmpty
                new object[] { GetDefaultPartner(), null },
                // EndDateLessThenCurrentDate
                new object[] { GetDefaultPartner(), GetDefaultPartnerPromoCodeLimitRequest(endDate: DateTime.Now.AddDays(-1)) },
                #endregion
            };
        }

        #endregion

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = new Fixture().Create<Guid>();
            _repositoryMock.Setup(q => q.GetByIdAsync(id)).ReturnsAsync((Partner)null);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(id, null);

            // Assert
            //// всё же немного классики:
            //Assert.IsType<NotFoundResult>(result);
            // FluentAssertions:
            result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [MemberData(nameof(GetBadRequestData))]
        public async Task SetPartnerPromoCodeLimitAsync_RequestIsNotCorrent_ReturnsBadRequest(Partner partner, SetPartnerPromoCodeLimitRequest request)
        {
            // Arrange
            var id = new Fixture().Create<Guid>();
            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(partner);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(id, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PreviousLimitNotCanceled_SetNumberIssuedPromoCodesToZero()
        {
            // Arrange
            var id = new Fixture().Create<Guid>();
            //var fixture = new Fixture();
            //var partner = fixture.Create<Partner>();
            //partner.NumberIssuedPromoCodes = 100;
            var partner = GetDefaultPartner(numberIssuedPromoCodes: 100);
            var request = GetDefaultPartnerPromoCodeLimitRequest();
            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(partner);

            // Act
            await _controller.SetPartnerPromoCodeLimitAsync(id, request);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PreviousLimitIsCanceled_DontSetNumberIssuedPromoCodesToZero()
        {
            // Arrange
            var id = new Fixture().Create<Guid>();
            int numberIssuedPromoCodes = 100;
            var partner = GetDefaultPartner(
                numberIssuedPromoCodes: numberIssuedPromoCodes,
                cancelDateOfPartnerPromoCodeLimit: DateTime.Now
            );
            var request = GetDefaultPartnerPromoCodeLimitRequest();
            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(partner);

            // Act
            await _controller.SetPartnerPromoCodeLimitAsync(id, request);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(numberIssuedPromoCodes);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PreviousLimitNotCanceled_CancelPreviousLimit()
        {
            // Arrange
            var id = new Fixture().Create<Guid>();
            var partner = GetDefaultPartner();
            var request = GetDefaultPartnerPromoCodeLimitRequest();
            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(partner);

            // Act
            await _controller.SetPartnerPromoCodeLimitAsync(id, request);

            // Assert
            partner.PartnerLimits.First().CancelDate.HasValue.Should().BeTrue();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_NewPartnerLimit_IsSaved()
        {
            #region Это вариант с Mock, поэтому тут не проверяем, выполнена ли запись реально
            //// Arrange
            //var id = new Fixture().Create<Guid>();
            //var partner = GetDefaultPartner();
            //var request = GetDefaultPartnerPromoCodeLimitRequest();
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(partner);

            //// Act
            //var result = await _controller.SetPartnerPromoCodeLimitAsync(id, request);

            //// Assert
            //// Проверяем что UpdateAsync был вызван, причём единожды
            //_repositoryMock.Verify(m => m.UpdateAsync(It.IsAny<Partner>()), Times.Exactly(1));
            //// Проверяем, что всё прошло успешно:
            //// 1) Проверяем ответ, возвращённый контроллером
            //result.Should().BeOfType<CreatedAtActionResult>();
            //// 2)
            //(result as CreatedAtActionResult).ActionName.Should().Be("GetPartnerLimitAsync");
            //(result as CreatedAtActionResult).RouteValues.Count.Should().Be(2);
            //(result as CreatedAtActionResult).RouteValues["id"].Should().Be(partner.Id);
            #endregion

            #region А это вариант, когда успешность записи в БД действительно проверяется
            // Arrange
            var id = GetDefaultPartner().Id; // Теперь нужен реальный ID, который есть в БД.
            var request = GetDefaultPartnerPromoCodeLimitRequest();
            DataContext context = new DataContext();
            EfDbInitializer dbInitializer = new EfDbInitializer(context);
            dbInitializer.InitializeDb();
            EfRepository<Partner> repository = new EfRepository<Partner>(context);
            PartnersController controller = new PartnersController(repository);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(id, request);

            // Assert
            // Проверяем, что запись в БД появилась:
            // 1) Проверяем ответ, возвращённый контроллером
            result.Should().BeAssignableTo<CreatedAtActionResult>(); // так гибче, чем BeOfType
            // 2) Проверяем, что ID содержащийся в ответе...
            Guid newGuid = (Guid)(result as CreatedAtActionResult).RouteValues["id"];
            // ... равен исходному
            newGuid.Should().Be(id);
            // 3) А так же находится через контекст в БД
            context.Set<Partner>().FirstOrDefaultAsync(x => x.Id == newGuid).Should().NotBeNull();
            #endregion
        }

        public void Dispose()
        {

        }

    }
}