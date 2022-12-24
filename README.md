# WPF  приложение мониторинга тендерных площадок для отдела закупок"

![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F%20%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0.png)

## Описание функционала программы
Приложение просматривает указанные пользователем сайты и по ключевым словам отбирает нужные тендеры.
Выбранные приложением тендеры попадают в итоговый отчёт.
В приложении используется механизм внедрения зависемостей LightInject, втраиваемая бд SQLite, взаимодействие с бд через EF.
Для работы с excel используется NPOI.

## Принцип работы

Пользователь указывает направление поиска. В зависимости от выбранного направления меняется список предлагаемых ключевых слов.


![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%B2%D1%8B%D0%B1%D0%BE%D1%80%20%D0%BA%D0%BB%D1%8E%D1%87%D0%B5%D0%B2%D1%8B%D1%85%20%D1%81%D0%BB%D0%BE%D0%B2.png)



## Далее указывается путь сохранения отчёта, выбираются площадки для поиска и нажимаем кнопку старт.



![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%BF%D1%80%D0%BE%D1%86%D0%B5%D1%81%D1%81.png)


![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%B8%D1%82%D0%BE%D0%B3.png)



## Отчёт согласно шаблона заказчина:

![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%BE%D1%82%D1%87%D1%91%D1%82.png)



## В приложении реализован механизм добавления и редактирования ключевых слов. Добавление реализовано пакетно (загружается файл-excel):


![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D1%80%D0%B5%D0%B4%D0%B0%D0%BA%D1%82%D0%BE%D1%80%20%D0%BA%D0%BB%D1%8E%D1%87%D0%B5%D0%B2%D1%8B%D1%85%20%D1%81%D0%BB%D0%BE%D0%B2.png)


## Редактирование слов:


![основная страница](https://github.com/E-A-Volobuev/TenderParserWpf/blob/master/%D0%BF%D1%80%D0%BE%D1%81%D0%BC%D0%BE%D1%82%D1%80%20%D0%B8%20%D1%80%D0%B5%D0%B4%D0%B0%D0%BA%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20%D0%BA%D0%BB%D1%8E%D1%87%D0%B5%D0%B2%D1%8B%D1%85%20%D1%81%D0%BB%D0%BE%D0%B2.png)
