#!/usr/bin/env python

#Dieses Softwarestück ist dazu gedacht auf dem Pi zu laufen,
# es nimmt Daten über einen UDS Socket/NamedPipe vom Client Programm welches ebenfalls auf dem Pi läuft
# und sendet sie via CANBus weiter.
# Spezifikationen des Dual CAN Hats: https://www.waveshare.com/wiki/2-CH_CAN_HAT
# Dort steht auch wie man das SPI Interface aktiviert, oder wie man die boot.config vom PI verändern muss.

import threading
import time
import can
import os
import struct
import sys
import errno
import socket

# C# scheint unter UNIX aus einer NamedPipe einen UDS Socket zu machen, deswegen erstellt die Server Software einen UDS Socket,
# deswegen verbinden wir uns hier mit einem UDS Socket
# UDS Socket ist auch bekannt als IPC Socket, und IPC steht für Inter Process Communication,
# in diesem Fall vermittelt er also zwischen diesem Code und der Server Software
# Der UDS Socket wird umgesetzt als eine Datei im System, diese hat insbesondere eine Adresse.
# Standartmäßig ist diese Adresse für einen Socket names 'test' : '/tmp/CoreFxPipe_test'

sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)

# os.system('code') emuliert im Grunde genommen einen User der die Befehle innerhalb der Klammer im Terminal ausführt,
# insbesondere bedeutet dies dass man die Befehle innerhalb der Klammern auch manuell ausführen kann

#initialisiere CANBus
os.system('sudo ip link set can0 type can bitrate 1000000')  #1mBit/s, sollte gleicher Wert wie im Arduino auch sein
os.system('sudo ifconfig can0 up')
os.system('sudo ifconfig can0 txqueuelen 1000')
#os.system('sudo ip link set can1 type can bitrate 1000000')  #1mBit/s, sollte gleicher Wert wie im Arduino auch sein
#os.system('sudo ifconfig can1 up')
#os.system('sudo ifconfig can1 txqueuelen 1000')
# Vorsicht: der can0 Ausdruck vor dem '=' ist eine Variable in unserem Programm,
# das channel='can0' bezieht sich auf den can0 der in der config.txt gesetzt wurde

can0 = can.interface.Bus(channel='can0', bustype='socketcan_ctypes')
#can1 = can.interface.Bus(channel='can1', bustype='socketcan_ctypes')


#Verbindung zum Server erstellen
try:
    print("verbinde mit NamedPipe")
    sock.connect('/tmp/CoreFxPipe_testpipe')
    print("NamedPipe verbunden")
    print("starte mainloop")
except socket.error as msg:
    print(msg)
    os.system('sudo ifconfig can0 down')
    sys.exit(1)


#Schickt die Daten vom Server zum CanBus
def server_to_can():
    while True:
        print("Thread 1 server->can")
        #Am Anfang jeder Nachricht steht ihre arbitrationID als 32 bit Int.
        #ArbitrationID wird anhand der im "Interface specification for airborne CAN applications V 1.7" empfohlenen Werte im Server vergeben.
        #Diese findet man unter https://www.stockflightsystems.com/canaerospace.html unter dem download 'canas_17.pdf'
        #Die arbitrationID bestimmt, wie die Daten zu interpretieren sind, zB dass die Bytes als Floatwert für MPH interpretiert werden sollen.
        busid = sock.recv(1, socket.MSG_WAITALL)
        busid = int.from_bytes(busid,"little", signed =False)

        arbid = sock.recv(4, socket.MSG_WAITALL)
        arbid = int.from_bytes(arbid, "little", signed=False)

        lenght = sock.recv(1, socket.MSG_WAITALL)
        lenght = int.from_bytes(lenght, byteorder='little', signed=False)

        data = sock.recv(lenght, socket.MSG_WAITALL)
        #print(busid, arbid, struct.unpack('f', data))
        #data = data[::-1]  #little endian server big endian arduino

        if busid == 0:
            msg = can.Message(arbitration_id=arbid, data=data, extended_id=False)
            can0.send(msg)
            print(msg)
            """if busid == 1:
                msg = can.Message(arbitration_id=arbid, data= data,extended_id=False)
                can1.send(msg)"""

#Schickt die Daten vom CanBus zum Server
def can_to_server():
    while True:
        print("Thread 2 can->server")
        #If you set the timeout to 0.0, the read will be executed as non-blocking, which means bus.recv(0.0) will return immediately, either with Message object or None,
        #depending on whether data was available on the socket. can0.recv() -> blocking (wartet bis eine Can message ankommt)

        #canbus == 0
        message = can0.recv()  # Timeout in seconds.
        if message is None:
            continue

        arbitrationID = message.arbitration_id
        messageData = message.data
        lenght = len(messageData)

        print(message)
        #(x) = struct.unpack('f', messageData)

        sock.sendall(struct.pack('>B', 0))
        sock.sendall(struct.pack("I", arbitrationID)) #bytearray(arbID))
        sock.sendall(struct.pack(">B", lenght))
        sock.sendall(messageData)
        #sock.sendall(struct.pack("f",x[0])) #bytearray(daten))

        #canbus == 1
        """message = can1.recv()  # Timeout in seconds.
        if message is None:
            continue

        arbitrationID = message.arbitration_id
        messageData = message.data
        lenght = len(messageData)

        print(message)
        (x) = struct.unpack('f', messageData)

        sock.sendall(struct.pack('I', 1))
        sock.sendall(struct.pack('I', arbitrationID)) #bytearray(arbID))
        sock.sendall(struct.pack("I", lenght))
        sock.sendall(struct.pack("f",x[0])) #bytearray(daten))"""




try:
    threading.Thread(target=server_to_can).start()
    threading.Thread(target=can_to_server).start()



except:            #Das Betriebssystem greift auf die Ressourcen wie den CANChip zu und blockiert diese für Verwendung durch andere Programme.
#Wenn wir den CANBus nicht wieder freigeben können wir ihn nicht benutzen, daher muss er geschlossen werden.
#Wenn das Programm abstürzt muss man dies ggf auch manuell über das Terminal tun.

    print('closing socket and CAN')
    sock.close()
    os.system('sudo ifconfig can0 down')
    #os.system('sudo ifconfig can1 down')
