namespace NaCl.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    using NUnit.Framework;

    using Base;
    using Internal;
    using Vectors;

    [TestFixture]
    public class XChaCha20Tests
    {
        private const string EXCEPTION_MESSAGE_NONCE_LENGTH = "The nonce length in bytes must be 24.";

        [Test]
        public void CreateInstanceWhenKeyLengthIsInvalidFails()
        {
            // Arrange, Act & Assert
            Assert.Throws<CryptographicException>(() => new XChaCha20(new byte[Snuffle.KEY_SIZE_IN_BYTES + TestHelpers.ReturnRandomPositiveNegative()], 0));
        }

        [Test]
        public void EncryptWhenNonceLengthIsInvalidFails()
        {
            // Arrange
            var cipher = new XChaCha20(new byte[Snuffle.KEY_SIZE_IN_BYTES], 0);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.Encrypt(new byte[0], new byte[cipher.NonceSizeInBytes() + TestHelpers.ReturnRandomPositiveNegative()]), EXCEPTION_MESSAGE_NONCE_LENGTH);
        }

        [Test]
        public void EncryptWhenNonceIsEmptyFails()
        {
            // Arrange
            var cipher = new XChaCha20(new byte[Snuffle.KEY_SIZE_IN_BYTES], 0);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.Encrypt(new byte[0], new byte[0]), EXCEPTION_MESSAGE_NONCE_LENGTH);
        }

        [Test]
        public void DecryptWhenNonceLengthIsInvalidFails()
        {
            // Arrange
            var cipher = new XChaCha20(new byte[Snuffle.KEY_SIZE_IN_BYTES], 0);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.Decrypt(new byte[0], new byte[cipher.NonceSizeInBytes() + TestHelpers.ReturnRandomPositiveNegative()]), EXCEPTION_MESSAGE_NONCE_LENGTH);
        }

        [Test]
        public void DecryptWhenNonceIsEmptyFails()
        {
            // Arrange
            var cipher = new XChaCha20(new byte[Snuffle.KEY_SIZE_IN_BYTES], 0);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.Decrypt(new byte[0], new byte[0]), EXCEPTION_MESSAGE_NONCE_LENGTH);
        }

        [Test]
        public void DecryptWhenCiphertextIsTooShortFails()
        {
            // Arrange
            var rnd = new Random();
            var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];

            // Act
            var cipher = new XChaCha20(key, 0);

            // Assert
            Assert.Throws<CryptographicException>(() => cipher.Decrypt(new byte[2]));
        }

        [Test]
        public void EncryptDecryptNBlocksTest()
        {
            // Arrange
            var rnd = new Random();
            var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];

            for (var i = 0; i < 64; i++)
            {
                rnd.NextBytes(key);

                var cipher = new XChaCha20(key, 0);

                for (var j = 0; j < 64; j++)
                {
                    var expectedInput = new byte[rnd.Next(300)];
                    rnd.NextBytes(expectedInput);

                    // Act
                    var output = cipher.Encrypt(expectedInput);
                    var actualInput = cipher.Decrypt(output);

                    // Assert
                    //Assert.AreEqual(expectedInput, actualInput);
                    Assert.IsTrue(CryptoBytes.ConstantTimeEquals(expectedInput, actualInput));
                }
            }
        }

        [Test]
        public void EncryptDecryptLongMessagesTest()
        {
            var rnd = new Random();

            var dataSize = 16;
            while (dataSize <= (1 << 24))
            {
                var plaintext = new byte[dataSize];
                rnd.NextBytes(plaintext);

                var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];
                rnd.NextBytes(key);

                var cipher = new XChaCha20(key, 0);

                var ciphertext = cipher.Encrypt(plaintext);
                var decrypted = cipher.Decrypt(ciphertext);

                //Assert.AreEqual(plaintext, decrypted);
                Assert.IsTrue(CryptoBytes.ConstantTimeEquals(plaintext, decrypted));
                dataSize += 5 * dataSize / 11;
            }
        }

        [Test]
        public void EncryptDecryptLongMessagesWithNonceTest()
        {
            var rnd = new Random();

            var dataSize = 16;
            while (dataSize <= (1 << 24))
            {
                var plaintext = new byte[dataSize];
                rnd.NextBytes(plaintext);

                var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];
                rnd.NextBytes(key);

                var cipher = new XChaCha20(key, 0);

                var nonce = new byte[cipher.NonceSizeInBytes()];
                rnd.NextBytes(nonce);

                var ciphertext = cipher.Encrypt(plaintext, nonce);
                var decrypted = cipher.Decrypt(ciphertext, nonce);

                //Assert.AreEqual(plaintext, decrypted);
                Assert.IsTrue(CryptoBytes.ConstantTimeEquals(plaintext, decrypted));
                dataSize += 5 * dataSize / 11;
            }
        }

        [Test]
        public void XChaCha20BlockWhenNonceLengthIsEmptyFails()
        {
            // Arrange
            var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];

            var cipher = new XChaCha20(key, 0);
            var nonce = new byte[0];
            var block = new byte[Snuffle.BLOCK_SIZE_IN_BYTES];

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.ProcessKeyStreamBlock(nonce, 0, block));
        }

        [Test]
        public void XChaCha20BlockWhenNonceLengthIsInvalidFails()
        {
            // Arrange
            var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];

            var cipher = new XChaCha20(key, 0);
            var nonce = new byte[cipher.NonceSizeInBytes() + TestHelpers.ReturnRandomPositiveNegative()];
            var block = new byte[Snuffle.BLOCK_SIZE_IN_BYTES];

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.ProcessKeyStreamBlock(nonce, 0, block));
        }

        [Test]
        public void XChaCha20BlockWhenLengthIsInvalidFails()
        {
            // Arrange
            var key = new byte[Snuffle.KEY_SIZE_IN_BYTES];

            var cipher = new XChaCha20(key, 0);
            var nonce = new byte[cipher.NonceSizeInBytes() + TestHelpers.ReturnRandomPositiveNegative()];
            var block = new byte[0];

            // Act & Assert
            Assert.Throws<CryptographicException>(() => cipher.ProcessKeyStreamBlock(nonce, 0, block));
        }

        [Test]
        public void HChaCha20TestVectors()
        {
            // Arrange
            foreach (var test in HChaCha20TestVector.HChaCha20TestVectors)
            {
                var cipher = new XChaCha20(test.Key, 0);

                // Act
                var output = cipher.HChaCha20(test.Input);

                // Assert
                //Assert.That(output, Is.EqualTo(test.Output));
                Assert.IsTrue(CryptoBytes.ConstantTimeEquals(test.Output, output));
            }
        }

        [Test]
        public void XChaCha20TestVectors()
        {
            // From libsodium's test/default/xchacha20.c (tv_stream_xchacha20) and https://tools.ietf.org/html/draft-arciszewski-xchacha-00.

            // Arrange
            foreach (var test in XChaCha20TestVector.XChaCha20TestVectors)
            {
                // Act
                var cipher = new XChaCha20(test.Key, 0);
                var output = cipher.Decrypt(CryptoBytes.Combine(test.Nonce, test.CipherText));

                // Assert
                //Assert.That(output, Is.EqualTo(test.Output));
                Assert.IsTrue(CryptoBytes.ConstantTimeEquals(test.PlainText, output));
            }
        }

        /*
        [Test]
        public void HChaCha20BlockTestVector()
        {
            // Arrange
            var key = CryptoBytes.FromHexString("00:01:02:03:04:05:06:07:08:09:0a:0b:0c:0d:0e:0f:10:11:12:13:14:15:16:17:18:19:1a:1b:1c:1d:1e:1f".Replace(":", string.Empty));
            var nonce = CryptoBytes.FromHexString("00:00:00:09:00:00:00:4a:00:00:00:00:31:41:59:27:00:00:00:00:00:00:00:00".Replace(":", string.Empty));

            // Act
            var output = XChaCha20.HChaCha20(key, nonce);
            var hex = CryptoBytes.ToHexStringLower(output); // is equal to 'expected' on the first and last rows...

            // Assert
            //var expected = new uint[8]
            //{
            //    0x82413b42, 0x27b27bfe, 0xd30e4250, 0x8a877d73,
            //    //0x4864a70a, 0xf3cd5479, 0x37cd6a84, 0xad583c7b,
            //    //0x8355e377, 0x127ce783, 0x2d6a07e0, 0xe5d06cbc,
            //    0xa0f9e4d5, 0x8a74a853, 0xc12ec413, 0x26d3ecdc
            //};

            var expected = new Array8<uint>
            {
                x0 = 0x82413b42,
                x1 = 0x27b27bfe,
                x2 = 0xd30e4250,
                x3 = 0x8a877d73,

                x4 = 0xa0f9e4d5,
                x5 = 0x8a74a853,
                x6 = 0xc12ec413,
                x7 = 0x26d3ecdc
            };

            Assert.AreEqual(expected, output.ToArray8());
        }
        */

        /*
        [Test]
        public void XChaCha20BlockTestVector()
        {
            // Arrange
            var key = CryptoBytes.FromHexString("00:01:02:03:04:05:06:07:08:09:0a:0b:0c:0d:0e:0f:10:11:12:13:14:15:16:17:18:19:1a:1b:1c:1d:1e:1f".Replace(":", string.Empty));
            var nonce = CryptoBytes.FromHexString("00:00:00:09:00:00:00:4a:00:00:00:00:31:41:59:27".Replace(":", string.Empty));
            var counter = 1;

            // Act
            var xchacha20 = new XChaCha20(key, 1);
            var output = xchacha20.GetKeyStreamBlock(nonce, counter);

            // Assert
            var expected = new uint[16]
            {
                0x82413b42, 0x27b27bfe, 0xd30e4250, 0x8a877d73,
                0x4864a70a, 0xf3cd5479, 0x37cd6a84, 0xad583c7b,
                0x8355e377, 0x127ce783, 0x2d6a07e0, 0xe5d06cbc,
                0xa0f9e4d5, 0x8a74a853, 0xc12ec413, 0x26d3ecdc,
            };

            Assert.AreEqual(expected, TestHelpers.ToUInt16Array(output));
        }
        */
    }
}
