﻿using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_SetLength_SessionOpen_FIleAccess
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private SftpFileAttributes _fileAttributes;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private MockSequence _sequence;
        private long _length;
        private NotSupportedException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            var random = new Random();
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = new[] {(byte) random.Next(byte.MinValue, byte.MaxValue)};
            _fileAttributes = SftpFileAttributes.Empty;
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(0, 1000);
            _writeBufferSize = (uint) random.Next(0, 1000);
            _length = random.Next();

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(_sequence).Setup(p => p.RequestFStat(_handle, false)).Returns(_fileAttributes);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.IsOpen)
                .Returns(true);

            _sftpFileStream = new SftpFileStream(
                _sftpSessionMock.Object,
                _path,
                FileMode.Create,
                FileAccess.Read,
                (int) _bufferSize);
        }

        protected void Act()
        {
            try
            {
                _sftpFileStream.SetLength(_length);
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void SetLengthShouldHaveThrownNotSupportedException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Write not supported.", _actualException.Message);
        }
    }
}
