=== "GatAvFwGyro"

    Firmware für Arduino CAN node, erstellt mit VS Code.
    Kann zur Demo direkt mit einem Knoten mit dem Beispielcode für Sin/Cos-Poti (Git-Hub) gekoppelt werden.

    ToBeDone:
        - Implemetierung CanAs-Service "std_identification"
        - Implementierung Vario, Bak, Turn und Airspeed (simple Analog Outs ggf. mit Justiermöglichkeit 0/max/Faktor)
        - Ablage aller einstellbaren Parameter im EEprom


=== "AddOn Gyro.pdf"

    Schaltplan Aufsteckplatine, Steckerbelegung Vario, Bank, Turn, Airspeed


=== "Gyro.FCStd"

    FreeCad-Datei der kompletten Mechanik. Daraus exportiert für 3D-Druck:
        - "Gyro-main frame.stl" (Motorträger)
        - "Gyro-cover.stl" (Gehäuse / Träger für Platine)
        - "Gyro-knob.stl" (Drehknopf)


