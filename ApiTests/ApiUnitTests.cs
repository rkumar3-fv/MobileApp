namespace ApiTests
{
    using System.Collections.Generic;
    using System.Linq;
    using FreedomVoice.Core;
    using FreedomVoice.Core.Entities.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ApiUnitTests
    {
        private readonly string _login;
        private readonly string _password;
        private readonly string _systemPhoneNumber;
        private readonly string _systemPhoneNumberOnHold;
        private readonly string _presentationPhoneNumber;
        private readonly int _mailBoxNumber;
        public ApiUnitTests()
        {
            _login = "freedomvoice.user1.267055@gmail.com";
            _password = "user1654654";
            _systemPhoneNumber = "7607124648";
            _systemPhoneNumberOnHold = "8477163106";
            _presentationPhoneNumber = "7606468294";
            _mailBoxNumber = 802;
        }

        [TestMethod]
        public void TestSuccessfulLogin()
        {
            Assert.AreEqual(ApiHelper.Login(_login, _password).Result.Code, ErrorCodes.Ok);
        }

        [TestMethod]
        public void TestWrongLoginFormat()
        {
            var wrongLogin = _login.Clone().ToString();
            wrongLogin = wrongLogin.Replace("@", "");
            Assert.AreEqual(ApiHelper.Login(wrongLogin, _password + " ").Result.Code, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public void TestWrongCredentialsCombinations()
        {
            Assert.AreEqual(ApiHelper.Login(_login, _password + " ").Result.Code, ErrorCodes.Unauthorized);
        }


        [TestMethod]
        public void TestPollingInterval()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsNotNull(ApiHelper.GetPollingInterval().Result.Result.PollingIntervalSeconds));

        }

        [TestMethod]
        public void TestSystemNumbers()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.AreEqual(ApiHelper.GetSystems().Result.Code, ErrorCodes.Ok));
        }

        [TestMethod]
        public void TestPresentationPhoneNumbers()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
            {
                Assert.IsTrue(ApiHelper.GetPresentationPhoneNumbers(_systemPhoneNumber).Result.Result.PhoneNumbers.Any());
            });
        }

        [TestMethod]
        public void TestCallReservations()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsNotNull(ApiHelper.CreateCallReservation(_systemPhoneNumber, _systemPhoneNumber, _presentationPhoneNumber, _presentationPhoneNumber).Result.Result.SwitchboardPhoneNumber));
        }

        [TestMethod]
        public void TestInvalidCallReservations()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.AreEqual(ApiHelper.CreateCallReservation(_systemPhoneNumber + "1", _systemPhoneNumber, _presentationPhoneNumber, _presentationPhoneNumber).Result.Code, ErrorCodes.BadRequest));
        }

        [TestMethod]
        public void TestGetMailboxes()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsTrue(ApiHelper.GetMailboxes(_systemPhoneNumber).Result.Result.Count > 0));
        }

        [TestMethod]
        public void TestGetMailboxesWithCount()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsTrue(ApiHelper.GetMailboxesWithCounts(_systemPhoneNumber).Result.Result.Count > 0));
        }


        [TestMethod]
        public void TestGetFolders()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsTrue(ApiHelper.GetFolders(_systemPhoneNumber, _mailBoxNumber).Result.Result.Count > 0));
        }


        [TestMethod]
        public void TestGetFoldersWithCounts()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.IsTrue(ApiHelper.GetFoldersWithCount(_systemPhoneNumber, _mailBoxNumber).Result.Result.Count > 0));
        }

        [TestMethod]
        public void TestGetFoldersWithCountsNumberOnHold()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.AreEqual(ApiHelper.GetFolders(_systemPhoneNumber, _mailBoxNumber).Result.Code, ErrorCodes.PaymentRequired));
        }

        [TestMethod]
        public void TestGetFoldersUnauthorized()
        {
            Assert.AreEqual(ApiHelper.GetFolders(_systemPhoneNumber, _mailBoxNumber).Result.Code, ErrorCodes.Unauthorized);
        }

        [TestMethod]
        public void TestGetMessages()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.AreEqual(ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "New", 10, 1, true).Result.Code, ErrorCodes.Ok));

        }

        [TestMethod]
        public void TestGetInvalidDataMessages()
        {
            ApiHelper.Login(_login, _password).GetAwaiter().OnCompleted(() =>
                Assert.AreEqual(ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "New1", 10, 1, true).Result.Code, ErrorCodes.BadRequest));

        }

        [TestMethod]
        public void TestGetMessagesUnauthorized()
        {
            Assert.AreEqual(ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "New", 10, 1, true).Result.Code, ErrorCodes.Unauthorized);
        }


        [TestMethod]
        public void TestMoveMessages()
        {
            ApiHelper.Login(_login, _password)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    var messages = ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "New", 10, 1, true).Result;
                    Assert.IsTrue(messages.Result.Any());

                    var messageId = messages.Result.First().Id;
                    Assert.AreEqual(ApiHelper.MoveMessages(_systemPhoneNumber, _mailBoxNumber, "Sent", new List<string> { messageId }).Result.Code, ErrorCodes.Ok);

                    var movedMessages = ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "Sent", 10, 1, true).Result;
                    Assert.IsTrue(movedMessages.Result.Any());
                    Assert.AreEqual(messageId, movedMessages.Result.First(x => x.Id == messageId));
                    Assert.AreEqual(ApiHelper.MoveMessages(_systemPhoneNumber, _mailBoxNumber, "New", new List<string> { messageId }).Result.Code, ErrorCodes.Ok);

                    var againMovedMessages = ApiHelper.GetMesages(_systemPhoneNumber, _mailBoxNumber, "New", 10, 1, true).Result;
                    Assert.IsTrue(messages.Result.Any());
                    Assert.AreEqual(messageId, againMovedMessages.Result.First(x => x.Id == messageId));
                }
                );
        }

    }
}
