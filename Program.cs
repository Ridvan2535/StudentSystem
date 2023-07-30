using System;
using System.Data.SqlClient;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace StudentSystem
{
    internal class Program
    {
        private static SqlConnection connection;
        
        static void Main(string[] args)
        {
            #region Connection
            string connectionString = @"Server=.\SQLEXPRESS; Database=StudentSystem; Trusted_Connection=SSPI; MultipleActiveResultSets=true; TrustServerCertificate=true;";
            connection = new SqlConnection(connectionString); //sql veri tabanına bağlanıyoruz.
            connection.Open();//Server Bağlantımızı açtıkk
            #endregion

            #region HASH
            Console.WriteLine("Lütfen Kullanıcıadı Giriniz:");
            string IdInput = Console.ReadLine();
            Console.WriteLine("Lütfen Şifre Giriniz:");
            string PasswordInput = MaskedPassword();
            string hashedPassword = GetHash(PasswordInput);
            #endregion

            #region Şifrelerin SQL'db sorgulaması
            SqlCommand komut = new SqlCommand();//SqlCommand Kullanarak db'deki dataları kullanabiliriz
            komut.Connection = connection;
            komut.CommandText = "SELECT * FROM Teachers WHERE NickName LIKE @id AND Password LIKE @password";
            //Teachers Tablosunde * ile tüm veriler arasında inputlarımızı Arıyoruz    
            //@ ile başlayan referanslar oluşturularak db'deki değerlere göre eşleme yapıyoruz.
            komut.Parameters.Add(new SqlParameter("id", IdInput));//parametrelerin değerleri değişkenlere eşleniyor
            komut.Parameters.Add(new SqlParameter("password", PasswordInput)); //parametrelerin değerleri değişkenlere eşleniyor

            SqlDataReader reader = komut.ExecuteReader();//sql de satır okuyoruz. ve değerleri "read"ile kullanabiliyoruz. 

            SqlCommand komut2 = new SqlCommand();
            komut2.Connection = connection;
            komut2.CommandText = "SELECT * FROM Students WHERE NickName LIKE @id AND Password LIKE @password";//like benzer demek

            //Students Tablosunde * ile tüm veriler arasında inputlarımızı Arıyoruz
            //@ ile başlayan referanslar oluşturularak db'deki Değerlere göre eşleme yapıyoruz
            komut2.Parameters.Add(new SqlParameter("id", IdInput));
            komut2.Parameters.Add(new SqlParameter("password", PasswordInput));

            SqlDataReader reader2 = komut2.ExecuteReader();//sql de satır okuyoruz. ve değerleri "reader"ile kullanabiliyoruz.
            #endregion

            #region öğretmen İşlemleri

            if (reader.Read())
            {
                Console.WriteLine($"Hoşgeldiniz {reader["Name"]} Hocam");
                while (true)
                {
                    Console.WriteLine("Lütfen yapmak istediğiniz işlemi seçiniz:");
                    Console.WriteLine("1 - Kayıtlı Öğrenci Not Girişi");
                    Console.WriteLine("2 - Yeni Ders Kaydı");
                    Console.WriteLine("3 - Yeni Öğrenci Kaydı");
                    Console.WriteLine("4 - Disiplin Kaydı");
                    Console.WriteLine("5 - Çıkış Yap");
                    string TeacherInput = Console.ReadLine();
                    switch (TeacherInput)
                    {
                        case "1":
                            AddNotes();
                            ClearAndSuccess();
                            break;

                        case "2":
                            AddLesson();
                            ClearAndSuccess();
                            break;

                        case "3":
                            AddStudent();
                            ClearAndSuccess();
                            break;

                        case "4":
                            AddDiscipine();
                            ClearAndSuccess();
                            break;

                        case "5":
                            Close();
                            return;
                    }
                }
            }
            #endregion

            #region Öğrenci İşlemleri
            else if (reader2.Read())
            {
                Console.WriteLine($"Hoşgeldin {reader2[3]}");
                Console.WriteLine("Lütfen yapmak istediğiniz işlemi seçiniz:");
                Console.WriteLine("1 - Notlarını Görüntüle");
                Console.WriteLine("2 - Çıkış Yap");
                string StudentInput = Console.ReadLine();
                switch (StudentInput)
                {
                    case "1":
                        ListNotes(reader2[0].ToString());
                        break;
                    case "2":
                        Close();
                        break;
                }
            }
            #endregion

            #region Yanlış Giriş
            else
            {
                Console.WriteLine("Kullanıcı Şifresi veya Id yanlış.");
                Console.WriteLine("Lütfen Tekrar Deneyiniz");
            }
            reader.Close();
            connection.Close();
            #endregion
        }

        private static string GetHash(string Input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        static void ListStudent()
        {
            SqlCommand studentListKomut = new SqlCommand();
            studentListKomut.Connection = connection;
            studentListKomut.CommandText = "SELECT * FROM Students";
            SqlDataReader studentReader = studentListKomut.ExecuteReader();
            Console.WriteLine("id\tname\tsurname");
            while (studentReader.Read())
            {
                Console.WriteLine($"{studentReader[0]}\t{studentReader[3]}\t{studentReader[4]}");
            }
        }
        static void AddNotes()
        {
            ListStudent();
            Console.WriteLine("Lütfen Not Girmek İstediğiniz Öğrenciyi Seçiniz:");
            string secim = Console.ReadLine();
            ListLessons();
            Console.WriteLine("Lütfen Not Eklemek İstediğiniz Dersi Seçiniz:");
            string dersSecim = Console.ReadLine();
            Console.WriteLine("Lütfen Notu Giriniz:");
            string Not = Console.ReadLine();

            SqlCommand addNoteKomut = new SqlCommand();
            addNoteKomut.Connection = connection;
            addNoteKomut.CommandText = "INSERT * INTO StudentNotes (StudentId,LessonId,Note) VALUES (@stundetId, @lessonId, @note)";
            addNoteKomut.Parameters.Add(new SqlParameter("@stundetId", secim));
            addNoteKomut.Parameters.Add(new SqlParameter("@lessonId", dersSecim));
            addNoteKomut.Parameters.Add(new SqlParameter("@note", Not));
            addNoteKomut.ExecuteNonQuery();
        }
        static void ListLessons()
        {
            SqlCommand lessonListKomut = new SqlCommand();
            lessonListKomut.Connection = connection;
            lessonListKomut.CommandText = "SELECT * FROM Lessons";
            SqlDataReader lessonReader = lessonListKomut.ExecuteReader();
            Console.WriteLine("id\tname");
            while (lessonReader.Read())
            {
                Console.WriteLine($"{lessonReader[0]}\t{lessonReader[1]}");
            }
        }
        static void AddLesson()
        {
            Console.WriteLine("Lütfen Eklemek İstediğiniz Dersin Adını Giriniz:");
            string lessonInput = Console.ReadLine();

            SqlCommand addLessonKomut = new SqlCommand();
            addLessonKomut.Connection = connection;
            addLessonKomut.CommandText = "INSERT INTO Lessons (LessonName) VALUES (@lessonName)";
            addLessonKomut.Parameters.Add(new SqlParameter("@lessonName", lessonInput));
            addLessonKomut.ExecuteNonQuery();
        }
        static void AddStudent()
        {
            Console.WriteLine("Yeni Öğrencinin Adı");
            string newStudentName = Console.ReadLine();
            Console.WriteLine("Yeni Öğrencinin Soyadı:");
            string newStudentSurname = Console.ReadLine();
            Console.WriteLine("Yeni Öğrencinin Kullanıcı Adı");
            string newNickName = Console.ReadLine();
            Console.WriteLine("Yeni Öğrencinin Kullanıcı Şifresi");
            string newPassword = Console.ReadLine();
            SqlCommand addNewStudent = new SqlCommand();
            addNewStudent.Connection = connection;
            addNewStudent.CommandText = "SELECT * FROM Students";
            addNewStudent.CommandText = "INSERT INTO Students (NickName, Password, Name, Surname) VALUES (@nickname, @password, @name, @surname)";
            addNewStudent.Parameters.Add(new SqlParameter("nickname", newNickName));
            addNewStudent.Parameters.Add(new SqlParameter("password", newPassword));
            addNewStudent.Parameters.Add(new SqlParameter("name", newStudentName));
            addNewStudent.Parameters.Add(new SqlParameter("surname", newStudentSurname));
            addNewStudent.ExecuteNonQuery();
        }
        static void AddDiscipine()
        {
            ListStudent();
            Console.WriteLine("Disiplin İşlemi Başlatmak İstediğiniz Öğrenciyi Seçiniz:");
            string studentId = Console.ReadLine();
            Console.WriteLine("Lütfen Disiplin Nedenini Giriniz");
            string whyDiscipine = Console.ReadLine();

            SqlCommand addNewDiscipine = new SqlCommand();
            addNewDiscipine.Connection = connection;
            addNewDiscipine.CommandText = "INSERT INTO Discipline(StudentId,Description) VALUES (@studentId,@description)";
            addNewDiscipine.Parameters.Add(new SqlParameter("studentId", studentId));
            addNewDiscipine.Parameters.Add(new SqlParameter("description", whyDiscipine));
            addNewDiscipine.ExecuteNonQuery();
        }
        static void ClearAndSuccess()
        {
            Console.Clear();
            Console.WriteLine("İşleminiz Başarıyla Gerçekleştirilmiştir.");
        }
        static void Close()
        {
            Console.WriteLine("Sistemden Güvenli Çıkış Yapılıyor...");
            Thread.Sleep(3000);
        }
        static void ListNotes(string studentid)
        {
            SqlCommand NoteReader = new SqlCommand();
            NoteReader.Connection = connection;
            NoteReader.CommandText = "SELECT * FROM StudentNotes sn JOIN Lessons ls ON sn.LessonId=ls.Id WHERE sn.StudentId=@id";
            NoteReader.Parameters.Add(new SqlParameter("id", studentid));
            SqlDataReader NotesReader = NoteReader.ExecuteReader();
            Console.WriteLine("Ders Adı\tNotun");
            while (NotesReader.Read())
            {
                Console.WriteLine($"{NotesReader[5]}\t{NotesReader[3]}");
            }
            Console.ReadLine();
        }
        static string MaskedPassword()
        {
            string PasswordKey = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter)
                {
                    PasswordKey += key.KeyChar;
                    Console.Write("*"); 
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine(); // Enter tuşuna basıldıktan sonra alt satıra geçmek için
            return PasswordKey;
        }
    }
}
