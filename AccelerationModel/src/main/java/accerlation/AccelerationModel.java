package accerlation;

import java.util.function.Function;

public class AccelerationModel {
    String typ;
    int maxGeschw;
    coordinates position;
    double höhe;
    int aktuelleGeschw;
    double a_x;
    double a_y;
    double a_z;
    double alpha;
    double beta;
    AccelerationModel(int maxGeschw){
        this.maxGeschw = maxGeschw;
        position = new coordinates(0,0,0);
        höhe = 0;
        aktuelleGeschw = 0;
    }

    void verbindeMitMicrosoftInterface(Function<AccelerationModel, String> interfaceFunktion){
        // Diese Methode verwendet eine funktionale Schnittstelle, um eine Verbindung zu Microsoft Interface herzustellen
        // Die interfaceFunktion ist ein Lambda-Ausdruck, der ein AccelerationModel als Parameter nimmt und einen String zurückgibt
        // Der String ist die Nachricht, die an Microsoft Interface gesendet wird
        // Zum Beispiel: interfaceFunktion = (AccelerationModel) -> "Typ: " + AccelerationModel.typ + ", Position: " + AccelerationModel.position + ", Höhe: " + AccelerationModelg.höhe;

        String nachricht = interfaceFunktion.apply(this); // Die interfaceFunktion wird auf das aktuelle Flugzeug angewendet
        System.out.println("Verbindung zu Microsoft Interface hergestellt. Nachricht gesendet: " + nachricht);
    }

    void berechneBeschleunigung(int kraft, int masse){
           ;
        // Diese Methode berechnet die Beschleunigung des Flugzeugs in den drei Achsen (x, y, z)
        // Die Kraft und die Masse sind die Parameter, die du eingeben musst
        // Die Formel für die Beschleunigung ist: a = F / m
        // Du musst die Kraft in die x-, y- und z-Komponenten aufteilen
        // Zum Beispiel: F_x = F * cos(alpha) * cos(beta), F_y = F * sin(alpha) * cos(beta), F_z = F * sin(beta)
        // alpha ist der Winkel in der xy-Ebene, beta ist der Winkel in der xz-Ebene
        // Du musst die Winkel berechnen oder eingeben

        coordinates x_Vektor = new coordinates(position.X,0,0);// Diese Werte muss von der Quelle automatisch erhalten
        coordinates y_Vektor = new coordinates(0 , position.Y , 0);// Diese Werte muss von der Quelle automatisch erhalten
        coordinates z_Vektor = new coordinates(0,0,position.Z);
        // Berchnet die VektorenProdukte :
        double x_VektorProdukty_Vektor = (x_Vektor.X*y_Vektor.X)+ (x_Vektor.Y * y_Vektor.Y) + (x_Vektor.Z * y_Vektor.Z) ;
        double x_VektorProduktz_Vektor = (x_Vektor.X* z_Vektor.X)+ (x_Vektor.Y * z_Vektor.Y) + (x_Vektor.Z * z_Vektor.Z) ;

        double x_VektorLang = Math.sqrt(x_Vektor.X * x_Vektor.X + x_Vektor.Y * x_Vektor.Y + x_Vektor.Z * x_Vektor.Z );
        double y_VektorLang = Math.sqrt(y_Vektor.X * y_Vektor.X   + y_Vektor.Y * y_Vektor.Y + y_Vektor.Z * y_Vektor.Z);
        double z_VektorLang = Math.sqrt(z_Vektor.X *z_Vektor.X +  z_Vektor.Y *z_Vektor.Y + z_Vektor.Z * z_Vektor.Z );

        // Winkel zwischen Achsen berechnen:

        this.alpha = Math.acos(x_VektorProdukty_Vektor/(x_VektorLang*y_VektorLang ));
        this.beta = Math.acos(x_VektorProduktz_Vektor/(x_VektorLang*z_VektorLang ));

        double a_x = kraft * Math.cos(this.alpha) * Math.cos(this.beta) / masse; // Die Beschleunigung in der x-Achse
        double a_y = kraft * Math.sin(this.alpha) * Math.cos(this.beta) / masse; // Die Beschleunigung in der y-Achse
        double a_z = kraft * Math.sin(this.beta) / masse; // Die Beschleunigung in der z-Achse
        this.a_x=a_x;
        this.a_y=a_y;
        this.a_z=a_z;

        System.out.println("Die Beschleunigung des Flugzeugs ist: " + a_x + " m/s^2 in der x-Achse, " + a_y + " m/s^2 in der y-Achse, " + a_z + " m/s^2 in der z-Achse.");
    }

    void aktualisierePosition(double deltaTime){
        // Diese Methode aktualisiert die Position des Flugzeugs in den drei Achsen (x, y, z)
        // Diese Methode kann vielleicht mit anderen Methoden von der Klasse Main.c hat, diese Methoden,
        // habe ich in der Klsse ConverteMethodenFromClassMainInterface
        // deltaTime ist der Zeitunterschied zwischen zwei Positionen
        //Die Formel für die Position ist: s = s_0 + v_0 * t + 0.5 * a * t^2
        // s_0 ist die Anfangsposition, v_0 ist die Anfangsgeschwindigkeit, a ist die Beschleunigung, t ist die Zeit
        // Du musst die Anfangsposition, die Anfangsgeschwindigkeit und die Beschleunigung in den drei Achsen kennen oder berechnen


        double s_x = position.X + aktuelleGeschw * Math.cos(this.alpha) * Math.cos(this.beta) * deltaTime + 0.5 * a_x * deltaTime * deltaTime; // Die Position in der x-Achse
        double s_y = position.Y + aktuelleGeschw * Math.sin(this.alpha) * Math.cos(this.beta) * deltaTime + 0.5 * a_y * deltaTime * deltaTime; // Die Position in der y-Achse
        double s_z = höhe + aktuelleGeschw * Math.sin(this.beta) * deltaTime + 0.5 * a_z * deltaTime * deltaTime; // Die Position in der z-Achse
        position = new coordinates(s_x, s_y,s_z); // Die neue Position wird gespeichert
        höhe =  s_z; // Die neue Höhe wird gespeichert
        System.out.println("Die Position des Flugzeugs ist: " + position + ", Die Höhe des Flugzeugs ist: " + höhe);
    }
    void zeigeErgebnis(){
        // Diese Methode zeigt die Beschleunigung und die Position des Flugzeugs auf dem Bildschirm an
        System.out.println("Die Beschleunigung des Flugzeugs ist: " + a_x + " m/s^2 in der x-Achse, " + a_y + " m/s^2 in der y-Achse, " + a_z + " m/s^2 in der z-Achse.");
        System.out.println("Die Position des Flugzeugs ist: " + position + ", Die Höhe des Flugzeugs ist: " + höhe);
    }

}
