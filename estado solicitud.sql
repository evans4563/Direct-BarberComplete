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
    Id_Rol INT NOT NULL,
    FOREIGN KEY (Id_Rol) REFERENCES Rol(Id)
);

DROP TABLE Solicitud;

DROP TABLE Usuario;


CREATE TABLE Rol (
    Id INT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);


INSERT INTO Rol (Nombre)
VALUES ('Cliente'), ('Barbero');


CREATE TABLE Solicitud (
    id_Solicitud INT IDENTITY(1,1) PRIMARY KEY,
    id_Cliente INT,
    id_Barbero INT,
	Dirección NVARCHAR(MAX),
    fecha DATETIME,
    descripcion NVARCHAR(200),
    precio DECIMAL(10,2),
    FOREIGN KEY (id_Cliente) REFERENCES Usuario(Id),
    FOREIGN KEY (id_Barbero) REFERENCES Usuario(Id)
);

select * from Usuario;

select * from Solicitud;


Select * from Solicitud;