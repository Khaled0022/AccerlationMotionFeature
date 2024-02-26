package accerlation;

public class coordinates {

    double X;
    double Y ;
    double Z;
    public double getX() {
        return X;
    }
    public coordinates(double x, double y, double z) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

  // Setter und Getter Methoden
    public void setX(double x) {
        X = x;
    }
    public void setY(double y) {
        Y = y;
    }
    public void setZ(double z) {
        Z = z;
    }
    public double getY() {
        return Y;
    }
    public double getZ() {
        return Z;
    }

}
