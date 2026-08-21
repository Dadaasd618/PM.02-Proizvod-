using ClinicApp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClinicApp.Tests
{
    [TestClass]
    public sealed class ValidationTests
    {
        // Тест 1: Проверка валидного имени
        [TestMethod]
        public void IsValidName_ValidName_ReturnsTrue()
        {
            // Arrange
            string name = "Иван Петров";

            // Act
            bool result = ValidationHelper.IsValidName(name);

            // Assert
            Assert.IsTrue(result, "Имя 'Иван Петров' должно быть валидным");
        }

        // Тест 2: Проверка пустого имени
        [TestMethod]
        public void IsValidName_EmptyName_ReturnsFalse()
        {
            // Arrange
            string name = "";

            // Act
            bool result = ValidationHelper.IsValidName(name);

            // Assert
            Assert.IsFalse(result, "Пустое имя должно быть невалидным");
        }

        // Тест 3: Проверка валидного полиса (16 цифр)
        [TestMethod]
        public void IsValidPolicy_Valid16Digits_ReturnsTrue()
        {
            // Arrange
            string policy = "1234567890123456";

            // Act
            bool result = ValidationHelper.IsValidPolicy(policy);

            // Assert
            Assert.IsTrue(result, "Полис из 16 цифр должен быть валидным");
        }

        // Тест 4: Проверка полиса с неверной длиной (15 цифр)
        [TestMethod]
        public void IsValidPolicy_InvalidLength_ReturnsFalse()
        {
            // Arrange
            string policy = "123456789012345";

            // Act
            bool result = ValidationHelper.IsValidPolicy(policy);

            // Assert
            Assert.IsFalse(result, "Полис из 15 цифр должен быть невалидным");
        }

        // Тест 5: Проверка полиса с буквами
        [TestMethod]
        public void IsValidPolicy_ContainsLetters_ReturnsFalse()
        {
            // Arrange
            string policy = "1234567890ABCDEF";

            // Act
            bool result = ValidationHelper.IsValidPolicy(policy);

            // Assert
            Assert.IsFalse(result, "Полис с буквами должен быть невалидным");
        }

        // Тест 6: Проверка валидной цены
        [TestMethod]
        public void IsValidPrice_ValidPrice_ReturnsTrue()
        {
            // Arrange
            string priceText = "500.50";

            // Act
            bool result = ValidationHelper.IsValidPrice(priceText, out decimal price);

            // Assert
            Assert.IsTrue(result, "Цена '500.50' должна быть валидной");
            Assert.AreEqual(500.50m, price, "Цена должна быть 500.50");
        }

        // Тест 7: Проверка невалидной цены (текст вместо числа)
        [TestMethod]
        public void IsValidPrice_InvalidPrice_ReturnsFalse()
        {
            // Arrange
            string priceText = "abc";

            // Act
            bool result = ValidationHelper.IsValidPrice(priceText, out decimal _);

            // Assert
            Assert.IsFalse(result, "Цена 'abc' должна быть невалидной");
        }
    }
}