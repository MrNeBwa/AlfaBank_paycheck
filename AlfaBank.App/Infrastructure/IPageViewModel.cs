using System.Windows;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Интерфейс модели представления, содержимое которой загружается при открытии страницы.
/// </summary>
public interface IPageViewModel
{
    /// <summary>
    /// Загружает данные страницы из источника данных.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию загрузки.</returns>
    Task LoadAsync();
}