CREATE DATABASE DirectBarber1;
USE DirectBarber1;

CREATE TABLE Usuario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50),
    apellido NVARCHAR(50),
    correo NVARCHAR(60) NOT NULL,
    contrasena NVARCHAR(200) NOT NULL,
    direccion NVARCHAR(50),
    telefono NVARCHAR(20),
    fec_registro DATETIME DEFAULT GETDATE(),
    fec_nacimiento DATE,
    calificacion DECIMAL(3,2),
    foto VARBINARY(MAX),
    documento NVARCHAR(10),
    id_Rol INT NOT NULL,
    FOREIGN KEY (id_Rol) REFERENCES Rol(id_Rol)
);


CREATE TABLE Rol (
    Id INT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);


INSERT INTO Rol (NombreRol)
VALUES ('Cliente'), ('Barbero');


/*Recomendacion de ChatGPT tabla solicitud*/
CREATE TABLE Solicitud (
    id_Solicitud INT IDENTITY(1,1) PRIMARY KEY,
    id_Cliente INT,
    id_Barbero INT,
    fecha DATETIME,
    descripcion NVARCHAR(200),
    precio DECIMAL(10,2),
    FOREIGN KEY (id_Cliente) REFERENCES Usuario(Id),
    FOREIGN KEY (id_Barbero) REFERENCES Usuario(Id)
);

/*Consulta para obtener el barbero.*/
SELECT u.nombre AS NombreBarbero, u.apellido AS ApellidoBarbero, u.telefono, s.fecha, s.descripcion, s.precio
FROM Solicitud s
INNER JOIN Usuario u ON s.id_Barbero = u.Id
WHERE s.id_Solicitud = @idSolicitud;



