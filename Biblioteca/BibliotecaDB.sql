Create DATABASE BibliotecaDB;
GO

USE BibliotecaDB;
GO

CREATE TABLE Libros (
    Id VARCHAR(50) PRIMARY KEY,
    Titulo NVARCHAR(100) NOT NULL,
    Autor NVARCHAR(100) NOT NULL,
    AñoPublicacion INT NOT NULL,
    NumeroPaginas INT NOT NULL,
    EstaDisponible BIT NOT NULL DEFAULT 1
);

CREATE TABLE Usuarios (
    NumeroSocio INT PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL
);

CREATE TABLE Prestamos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LibroId VARCHAR(50) NOT NULL,
    NumeroSocio INT NOT NULL,
    FechaPrestamo DATETIME NOT NULL,
    FechaDevolucion DATETIME,
    FOREIGN KEY (LibroId) REFERENCES Libros(Id),
    FOREIGN KEY (NumeroSocio) REFERENCES Usuarios(NumeroSocio)
);

SELECT * FROM Usuarios;
Select * From Libros;
Select * From Prestamos;
 

