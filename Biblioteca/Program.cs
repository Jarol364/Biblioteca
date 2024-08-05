using System;
using System.Data.SqlClient;
using System.Collections.Generic;

public abstract class Publicacion
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string Id { get; set; }
    public int AñoPublicacion { get; set; }

    public abstract void MostrarInformacion();
}

public interface IPrestable
{
    bool EstaDisponible { get; set; }
    void Prestar();
    void Devolver();
}

public class Libro : Publicacion, IPrestable
{
    public int NumeroPaginas { get; set; }
    public bool EstaDisponible { get; set; } = true;

    public override void MostrarInformacion()
    {
        Console.WriteLine($"Libro: {Titulo} por {Autor}, ID: {Id}, Año: {AñoPublicacion}, Páginas: {NumeroPaginas}");
    }

    public void Prestar()
    {
        if (EstaDisponible)
        {
            EstaDisponible = false;
            Console.WriteLine($"El libro '{Titulo}' ha sido prestado.");
        }
        else
        {
            Console.WriteLine($"El libro '{Titulo}' no está disponible en este momento.");
        }
    }

    public void Devolver()
    {
        EstaDisponible = true;
        Console.WriteLine($"El libro '{Titulo}' ha sido devuelto.");
    }
}

public class Usuario
{
    public int NumeroSocio { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }

    public Usuario(string nombre, string apellido, int numeroSocio)
    {
        Nombre = nombre;
        Apellido = apellido;
        NumeroSocio = numeroSocio;
    }
}

public class Biblioteca
{
    private string connectionString = @"Data Source=DESKTOP-HPLJ6F6\SQLEXPRESS;Initial Catalog=BibliotecaDB;Integrated Security=True";

    public void AgregarLibro(Libro libro)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO Libros (Id, Titulo, Autor, AñoPublicacion, NumeroPaginas, EstaDisponible) VALUES (@Id, @Titulo, @Autor, @AñoPublicacion, @NumeroPaginas, @EstaDisponible)";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", libro.Id);
            command.Parameters.AddWithValue("@Titulo", libro.Titulo);
            command.Parameters.AddWithValue("@Autor", libro.Autor);
            command.Parameters.AddWithValue("@AñoPublicacion", libro.AñoPublicacion);
            command.Parameters.AddWithValue("@NumeroPaginas", libro.NumeroPaginas);
            command.Parameters.AddWithValue("@EstaDisponible", libro.EstaDisponible);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                Console.WriteLine($"Libro '{libro.Titulo}' agregado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar el libro: {ex.Message}");
            }
        }
    }

    public void RegistrarUsuario(Usuario usuario)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO Usuarios (NumeroSocio, Nombre, Apellido) VALUES (@NumeroSocio, @Nombre, @Apellido)";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@NumeroSocio", usuario.NumeroSocio);
            command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            command.Parameters.AddWithValue("@Apellido", usuario.Apellido);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                Console.WriteLine($"Usuario registrado: {usuario.Nombre} {usuario.Apellido}, Número de socio: {usuario.NumeroSocio}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar el usuario: {ex.Message}");
            }
        }
    }

    public void PrestarLibro(string id, int numeroSocio)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Verificar disponibilidad del libro
                string checkQuery = "SELECT EstaDisponible FROM Libros WHERE Id = @Id";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection, transaction);
                checkCommand.Parameters.AddWithValue("@Id", id);
                bool estaDisponible = (bool)checkCommand.ExecuteScalar();

                if (!estaDisponible)
                {
                    Console.WriteLine("El libro no está disponible en este momento.");
                    transaction.Rollback();
                    return;
                }

                // Actualizar estado del libro
                string updateQuery = "UPDATE Libros SET EstaDisponible = 0 WHERE Id = @Id";
                SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction);
                updateCommand.Parameters.AddWithValue("@Id", id);
                updateCommand.ExecuteNonQuery();

                // Registrar el préstamo
                string insertQuery = "INSERT INTO Prestamos (LibroId, NumeroSocio, FechaPrestamo) VALUES (@LibroId, @NumeroSocio, @FechaPrestamo)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction);
                insertCommand.Parameters.AddWithValue("@LibroId", id);
                insertCommand.Parameters.AddWithValue("@NumeroSocio", numeroSocio);
                insertCommand.Parameters.AddWithValue("@FechaPrestamo", DateTime.Now);
                insertCommand.ExecuteNonQuery();

                transaction.Commit();
                Console.WriteLine("Libro prestado con éxito.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error al prestar el libro: {ex.Message}");
            }
        }
    }

    public void DevolverLibro(string id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Actualizar estado del libro
                string updateLibroQuery = "UPDATE Libros SET EstaDisponible = 1 WHERE Id = @Id";
                SqlCommand updateLibroCommand = new SqlCommand(updateLibroQuery, connection, transaction);
                updateLibroCommand.Parameters.AddWithValue("@Id", id);
                updateLibroCommand.ExecuteNonQuery();

                // Actualizar el préstamo
                string updatePrestamoQuery = "UPDATE Prestamos SET FechaDevolucion = @FechaDevolucion WHERE LibroId = @LibroId AND FechaDevolucion IS NULL";
                SqlCommand updatePrestamoCommand = new SqlCommand(updatePrestamoQuery, connection, transaction);
                updatePrestamoCommand.Parameters.AddWithValue("@FechaDevolucion", DateTime.Now);
                updatePrestamoCommand.Parameters.AddWithValue("@LibroId", id);
                updatePrestamoCommand.ExecuteNonQuery();

                transaction.Commit();
                Console.WriteLine("Libro devuelto con éxito.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error al devolver el libro: {ex.Message}");
            }
        }
    }

    public void MostrarLibrosDisponibles()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Libros WHERE EstaDisponible = 1";
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                Console.WriteLine("Libros disponibles:");
                while (reader.Read())
                {
                    Console.WriteLine($"Libro: {reader["Titulo"]} por {reader["Autor"]}, ID: {reader["Id"]}, Año: {reader["AñoPublicacion"]}, Páginas: {reader["NumeroPaginas"]}");
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al mostrar los libros disponibles: {ex.Message}");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Biblioteca biblioteca = new Biblioteca();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Gestión de Biblioteca ---");
            Console.WriteLine("1. Registrar Usuario");
            Console.WriteLine("2. Agregar Libro");
            Console.WriteLine("3. Mostrar Libros Disponibles");
            Console.WriteLine("4. Prestar Libro");
            Console.WriteLine("5. Devolver Libro");
            Console.WriteLine("6. Salir");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    Console.Write("Nombre: ");
                    string nombre = Console.ReadLine();
                    Console.Write("Apellido: ");
                    string apellido = Console.ReadLine();
                    Console.Write("Número de Socio: ");
                    int numeroSocio = int.Parse(Console.ReadLine());
                    biblioteca.RegistrarUsuario(new Usuario(nombre, apellido, numeroSocio));
                    break;
                case "2":
                    Console.Write("ID: ");
                    string id = Console.ReadLine();
                    Console.Write("Título: ");
                    string titulo = Console.ReadLine();
                    Console.Write("Autor: ");
                    string autor = Console.ReadLine();
                    Console.Write("Año de Publicación: ");
                    int anio = int.Parse(Console.ReadLine());
                    Console.Write("Número de Páginas: ");
                    int paginas = int.Parse(Console.ReadLine());
                    biblioteca.AgregarLibro(new Libro { Id = id, Titulo = titulo, Autor = autor, AñoPublicacion = anio, NumeroPaginas = paginas });
                    break;
                case "3":
                    biblioteca.MostrarLibrosDisponibles();
                    break;
                case "4":
                    Console.Write("ID del libro: ");
                    string idPrestamo = Console.ReadLine();
                    Console.Write("Número de Socio: ");
                    int socio = int.Parse(Console.ReadLine());
                    biblioteca.PrestarLibro(idPrestamo, socio);
                    break;
                case "5":
                    Console.Write("ID del libro a devolver: ");
                    string idDevolucion = Console.ReadLine();
                    biblioteca.DevolverLibro(idDevolucion);
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
    }
}