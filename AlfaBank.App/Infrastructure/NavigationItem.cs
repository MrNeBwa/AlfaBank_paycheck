namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Пункт меню навигации: заголовок и открываемая страница.
/// </summary>
/// <param name="Title">Заголовок пункта меню.</param>
/// <param name="Page">Модель представления страницы.</param>
public sealed record NavigationItem(string Title, object Page);
