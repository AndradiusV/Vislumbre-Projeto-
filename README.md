# Vislumbre (Projeto)
Este projeto foi feito na Unity 2022.3.62f3. Você tem permissão para fazer o que quiser com este projeto.


Você é livre para fazer o que bem entender com a estrutura do projeto, use como se fosse seu mas saiba que eu ficaria feliz de ser creditado.
Fique atento que as artes não foram feitas por mim, para uso delas é necessário a permissão de L. Fallen Yokai B.A. 

Sobre o projeto:
Está tudo uma bagunça, MEU DEUS. foi a primeira vez que eu fiz algo do tipo  então muitas partes não estão comentadas ou estão com comentários extras.
Muita coisa mudou durante a produção então a própria estrutura está  a nível de "primeiro projeto do tipo", qualquer dúvida entre em contato que eu irei te ajudar ao máximo!

A estrutura de conexão é parecida com a de um servidor. um dos clientes vai mimetizar o comportamento de um servidor.

Toda estrutura está em torno no IPv4, existem verificações do tipo "este ip passado é válido?" em alguns lugares cruciais.

Em  todo o código já estão integradas conexões por TCP, UDP e WebRTC (Video e dataChannel).
as 3 seguem o esquema de Servidor e clientes.
O TPC é a primeira conexão feita para estabelecer uma comunicação com os outros canais.
O UDP e WebRTC usa o TCP para fazer a conexão inicial.

depois de tudo estar estabelecido o TCP servirá para comando de controle e administração entre os clientes no servidor.
UP servirá para trocas simples e rápidas de informações como o que o mouse de cada um está fazendo, além de fazer a transmissão do audio.
WebRTC servirá para trocar o video.

O host irá controlar e adiministrar o script "Server.cs" enquanto os clientes adiministram o "Local_Client.cs", 
cada cliente terá uma instancia de "Client.cs" o representando  no lado do servidor, se quer mandar algo para algum cliente, é nesse script que você vai mirar.
Para permitir mais de 10 clientes ao mesmo tempo basta alterar uma variável no "Server.cs".


Sobre a captura de tela e audio.
Para capturar a tela eu useo o UWindowsCapture, ele captura a tela pelo script que eu fiz "captura_de_tela.cs".
a captura de audio é feita pelas funcionalidades do windows 11, porem são feitas por um programa externo já que para acessa-las foi preciso escrever em C++.

Sobre o updater:
O updater se conecta ao repo aqui no Git e baixa apenas os arquivos do vislumbre, ele ignora alguns arquivos .log e update.*.

De forma geral esses são os mais importantes.
