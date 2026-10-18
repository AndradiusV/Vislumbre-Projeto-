using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using uWindowCapture;

namespace GameServer
{
    internal static class Send_Client_Packet
    {
        #region Basico
        internal static void Client_Saudação_Para_Servidor()
        {
            Debug.Log("Enviei saudação para o servidor");
            using (Packet p = new Packet((int)ClientPackets.saudação_para_servidor))
            {
                //p.Write(Local_Client.instance.myId);

                SendTCPData_To_Server(p);
            }
        }
        internal static void Pingar()
        {
            Debug.Log("Pinguei");
            using (Packet p = new Packet((int)ClientPackets.pingar))
            {
                //p.Write(Local_Client.instance.myId);
                p.Write(SoundManager.instance.Ping_Id());

                SendTCPData_To_Server(p);
            }
        }
        public static void UDPTestReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.updTestReceived))
            {
                _packet.Write("Received a UDP packet.");

                SendUDPData_To_Server(_packet);
            }
        }
        #endregion
        #region Pedidos
        internal static void pedir_Para_Transmitir(bool intenção)
        {
            using (Packet p = new Packet((int)ClientPackets.pedir_Para_Transmitir))
            {
                //p.Write(Local_Client.instance.myId);
                p.Write(intenção);
                if (intenção)
                {
                    //Manda primeiro o tamanho da textura de desenho
                    Desenho_Na_Tela.Iniciar_Transmitir();
                    int width = Desenho_Na_Tela.width;
                    int height = Desenho_Na_Tela.height;
                    p.Write(width);
                    p.Write(height);
                    p.Write((int)Desenho_Na_Tela.permi_Desenho);
                }

                SendTCPData_To_Server(p);
            }
        }
        internal static void Pedir_Para_Ver_Tela(bool intenção)
        {
            using (Packet p = new Packet((int)ClientPackets.pedir_para_Ver_Tela))
            {
                p.Write(intenção);

                SendTCPData_To_Server(p);
            }
        }
        #endregion
        #region Informar
        internal static void Informar_Sore_Tela_Escolhida(bool escolheu)
        {
            using (Packet p = new Packet((int)ClientPackets.informar_Tela_Escolhida))
            {
                p.Write(escolheu);

                SendTCPData_To_Server(p);
            }
        }
        internal static void Informar_Permi_Desenho(int permi)
        {
            using (Packet p = new Packet((int)ClientPackets.informar_Permi_Desenho))
            {
                p.Write(permi);

                SendTCPData_To_Server(p);
            }
        }
        #endregion
        #region Envios
        internal static void Enviar_Tela(byte[] data)
        {
            if (Captura_De_Tela.instance != null && Captura_De_Tela.instance.window != null &&
                Captura_De_Tela.instance.window.texture != null)
            {
                SendUDPData_To_Server(data);
            }
        }
        //internal static void Enviar_Tela(byte[] data)
        //{
        //    if (Captura_De_Tela.instance != null && Captura_De_Tela.instance.window != null &&
        //        Captura_De_Tela.instance.window.texture != null)
        //    {
        //        using (Packet p = new Packet((int)ClientPackets.envio_de_tela))
        //        {
        //            p.Write(Local_Client.instance.myId);
        //            p.Write(data);

        //            SendUDPData_To_Server(p);
        //        }
        //    }
        //}
        internal static void Enviar_Som(byte[] data)
        {
            using (Packet p = new Packet((int)ClientPackets.envio_de_Som))
            {
                //p.Write(Local_Client.instance.myId);
                p.Write(data);

                SendUDPData_To_Server(p);
            }
        }
        internal static void Envio_Mouse(BrushType tipo, Mouse_Type mouse_Type, Permissão_De_Desenho tipo_Desenho, Vector2 pos, Color cor, float size)
        {
            using (Packet p = new Packet((int)ClientPackets.envio_de_mouse))
            {
                //p.Write(Local_Client.instance.myId);
                p.Write((int)tipo);
                p.Write((int)tipo_Desenho);
                p.Write((int)mouse_Type);
                p.Write(pos);
                p.Write(cor);
                p.Write(size);

                SendUDPData_To_Server(p);
            }
        }
        #endregion

        #region Conn Funcs
        private static void SendTCPData_To_Server(Packet _packet)
        {
            _packet.WriteLength();
            Local_Client.instance.tcp.SendData(_packet);
        }
        private static void SendUDPData_To_Server(Packet _packet)
        {
            //_packet.WriteLength();
            Local_Client.instance.udp.SendData(_packet);
        }
        private static void SendUDPData_To_Server(byte[] _packet)
        {
            //_packet.WriteLength();
            Local_Client.instance.udp.SendData(_packet);
        }
        #endregion
    }
}
