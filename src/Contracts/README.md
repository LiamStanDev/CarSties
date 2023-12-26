### Why we need Contracts ?
因為 microservice 使用 Message Bus 進行溝通，兩者會定義一個合約，
用於傳遞消息，因為我們所有的 microservices 都是使用 c# 編寫，故
我們可以建立一個統一的合約 project，不然要在每個 microservice 都
添加上相同的 Contract。但使用不同語言就沒辦法這樣做。
