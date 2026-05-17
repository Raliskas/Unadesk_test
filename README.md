Unadesk\_test


https://docs.google.com/document/d/1GKmRARVykfB7zqTW74\_ISGR9RqFYJg3wR74pZLCX3Do/edit?tab=t.0


Init - папка с init.sql для инициализации бд с созданными пустыми Табличками 



Src - основная папка 
infrastructure -> Data -> модельки и если бы реализовал через CQRS то хендлеры 

infrastructure -> работа с бд 
integration -> RabbitMQ продюсер и консюмер

presentation -> Public\_Api - минимал апи реализация с логикой обработки внутри 
		Worker - обработчик сообщений из RabbitMq

Не стал реализовывать все через CQRS или Repository тк тестовое задание и я так понял тут был основной упор был с RabbitMq , до этого с ним не работал , а только с Kafka

В описании не было сказано про гарантию доставки сообщения, поэтому не стал реализовывать паттерн Transaction Outbox 

