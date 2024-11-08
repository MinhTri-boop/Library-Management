using System.Net;
using System.Reflection;
using System.Xml.Serialization;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryManagementSystem
{
    // Models
    public class BaseEntity
    {
        private int id;
        public int Id { get => id; set => id = value; }
    }
    public class Book: BaseEntity
    {
        private string title;
        private string publisher;
        private string author;
        private int quantity;
        public string Title { get => title; set => title = value; }
        public string Publisher { get => publisher; set => publisher = value; }
        public string Author { get => author; set => author = value; }
        public int Quantity { get => quantity; set => quantity = value; }
        public bool IsAvailable => Quantity > 0;

        public Book() { }

        public Book(string title, string publisher, string author, int quantity)
        {
            Title = title;
            Publisher = publisher;
            Author = author;
            Quantity = quantity;
        }
    }

    public class Member: BaseEntity
    {
        private string name;
        private DateTime dateofbirth;
        public string Name { get => name; set => name = value; }
        public DateTime DateOfBirth { get => dateofbirth; set => dateofbirth = value; }
        public List<Book> BorrowedBooks { get; set; }
        public List<FineTicket> Fines { get; set; }

        public Member()
        {
            BorrowedBooks = new List<Book>();
            Fines = new List<FineTicket>();
        }

        public Member(string name, DateTime dateOfBirth)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
            BorrowedBooks = new List<Book>();
            Fines = new List<FineTicket>();
        }
    }

    public class FineTicket: BaseEntity
    {
        private int memberid;
        private string reason;
        private decimal amount;
        private bool ispaid;
        private DateTime issuedate;
        public int MemberID { get => memberid ; set => memberid = value; }
        public string Reason { get => reason; set => reason = value; }
        public decimal Amount { get => amount; set => amount = value; }
        public bool IsPaid { get => ispaid; set => ispaid = value; }
        public DateTime IssuedDate { get => issuedate; set => issuedate =value; }

        public FineTicket() { }

        public FineTicket(int memberID, string reason, decimal amount)
        {
            MemberID = memberID;
            Reason = reason;
            Amount = amount;
            IsPaid = false;
            IssuedDate = DateTime.Now;
        }
    }

    public class Manager: BaseEntity
    {

        private string username;
        private string password;
        private string fullname;
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string FullName { get => fullname; set => fullname = value; }

        public Manager() { }

        public Manager(string username, string password, string fullName)
        {
            Username = username;
            Password = password;
            FullName = fullName;
        }
    }

    // Data Access
    public static class DataManager
    {
        private static string managersPath = "managers.xml";
        private static string membersPath = "members.xml";
        private static string booksPath = "books.xml";
        private static string finesPath = "fines.xml";

        public static void SaveData(List<Manager> managers, List<Member> members, List<Book> books, List<FineTicket> fines)
        {
            SaveToXml(managers, managersPath);
            SaveToXml(members, membersPath);
            SaveToXml(books, booksPath);
            SaveToXml(fines, finesPath);
        }

        public static (List<Manager> managers, List<Member> members, List<Book> books, List<FineTicket> fines) LoadData()
        {
            List<Manager> managers = LoadFromXml<List<Manager>>(managersPath) ?? new List<Manager>();
            List<Member> members = LoadFromXml<List<Member>>(membersPath) ?? new List<Member>();
            List<Book> books = LoadFromXml<List<Book>>(booksPath) ?? new List<Book>();
            List<FineTicket> fines = LoadFromXml<List<FineTicket>>(finesPath) ?? new List<FineTicket>();

            return (managers, members, books, fines);
        }

        private static void SaveToXml<T>(T data, string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to {filePath}: {ex.Message}");
            }
        }

        private static T LoadFromXml<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    return serializer.Deserialize(stream) as T;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from {filePath}: {ex.Message}");
                return null;
            }
        }
    }

    // Business Logic
    public class Library
    {
        private static Library _instance;
        private List<Manager> managers;
        private List<Member> members;
        private List<Book> books;
        private List<FineTicket> fines;

        private Library()
        {
            LoadData();
        }

        public static Library GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Library();
            }
            return _instance;
        }

        private void LoadData()
        {
            (List<Manager> managers, List<Member> members, List<Book> books, List<FineTicket> fines) data = DataManager.LoadData();
            managers = data.managers;
            members = data.members;
            books = data.books;
            fines = data.fines;

            if (managers.Count == 0)
            {
                managers.Add(new Manager("admin", "admin123", "Administrator"));
                SaveData();
            }
        }

        private void SaveData()
        {
            DataManager.SaveData(managers, members, books, fines);
        }

        public Manager Login(string username, string password)
        {
            Manager existingManager = null;
            foreach (Manager m in managers)
            {
                if (m.Username == username && m.Password == password)
                {
                    existingManager = m;
                    return existingManager;
                    break; // Dừng vòng lặp khi tìm thấy thành viên
                }
            }
            return existingManager;
        }

        public List<Member> GetAllMembers()
        {
            return members;
        }

        public Member FindMemberByID(int ID)
        {
            foreach (Member m in members)
            {
                if (m.Id == ID)
                {
                    return m;
                }
            }
            return null;
        }

        public void AddMember(Member member)
        {
            if (members.Count > 0)
            {
                int maxId = 0;
                foreach (Member m in members)
                {
                    if (m.Id > maxId)
                    {
                        maxId = m.Id;
                    }
                }
                member.Id = maxId + 1;
            }
            else
            {
                member.Id = 1;
            }
            members.Add(member);
            SaveData();
        }

        public void UpdateMember(Member member)
        {
            Member existingMember = null;
            foreach (Member m in members)
            {
                if (m.Id == member.Id)
                {
                    existingMember = m;
                    break; // Dừng vòng lặp khi tìm thấy thành viên
                }
            }
            if (existingMember != null)
            {
                existingMember.Name = member.Name;
                existingMember.DateOfBirth = member.DateOfBirth;
                SaveData();
            }
        }

        public List<FineTicket> GetAllFines()
        {
            return fines;
        }

        public List<Book> GetAllBooks()
        {
            return books;
        }

        public List<Book> GetAllBorrowBook(Member member)
        {
            return member.BorrowedBooks;
        }
        public List<Book> FindBookByTitle(string title)
        {
            List<Book> result = new List<Book>();

            foreach (Book b in books)
            {
                if (b.Title.ToLower() == title.ToLower())
                {
                    result.Add(b);
                }
            }

            return result;
        }
        public Book FindBookByID(int id)
        {
            Book book = null;
            foreach (Book b in books)
            {
                if (b.Id == id)
                {
                    book = b;
                    break;
                }
            }
            return book;
        }

        public void AddBook(Book book)
        {
            if (books.Count > 0)
            {
                int maxId = 0;
                foreach (Book b in books)
                {
                    if (b.Id > maxId)
                    {
                        maxId = b.Id;
                    }
                }
                book.Id = maxId + 1;
            }
            else
            {
                book.Id = 1;
            }
            books.Add(book);
            SaveData();
        }

        public void UpdateBook(Book book)
        {
            Book existingBook = null;
            foreach (Book b in books)
            {
                if (b.Id == book.Id)
                {
                    existingBook = b;
                    break;
                }
            }

            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.Publisher = book.Publisher;
                existingBook.Author = book.Author;
                existingBook.Quantity = book.Quantity;
                SaveData();
            }
        }

        public void RemoveBook(int bookId)
        {
            Book book = null;
            foreach (Book b in books)
            {
                if (b.Id == bookId)
                {
                    book = b;
                    break;
                }
            }

            if (book != null)
            {
                books.Remove(book);
                SaveData();
            }
        }

        public bool BorrowBook(Member member, Book book)
        {
            if (book.IsAvailable)
            {
                book.Quantity--;
                member.BorrowedBooks.Add(book);
                SaveData();
                return true;
            }
            return false;
        }

        public bool ReturnBook(Member member, Book book)
        {
            bool found = false;
            Book bookREFtoRemove = null;
            foreach (Book b in member.BorrowedBooks)
            {
                if (b.Id == book.Id)  
                {
                    found = true;
                    bookREFtoRemove = b;
                    break;
                }
            }

            if (found)
            {
                
                member.BorrowedBooks.Remove(bookREFtoRemove);
                book.Quantity++;
                SaveData();
            }
            return found;
        }

        public void AddFine(FineTicket fine, int memberID)
        {
            int maxId = 1;
            if (fines.Count > 0)
            {
                maxId = fines[0].Id;
                foreach (FineTicket f in fines)
                {
                    if (f.Id > maxId)
                    {
                        maxId = f.Id;
                    }
                }
                fine.Id = maxId + 1;
            }
            else
            {
                fine.Id = 1;
            }

            fines.Add(fine);
            fine.MemberID = memberID;
            SaveData();
        }
        
        public FineTicket FindFineTicket(int ID) 
        {
            FineTicket fine = null;
            foreach (FineTicket f in fines)
            {
                if (f.Id == ID)
                {
                    fine = f;
                    break;
                }
            }
            return fine;
        }
        public void PayFine(FineTicket fine)
        {
            fine.IsPaid = true;
            SaveData();
        }
    }

    // Factory for creating FineTickets
    public class FineTicketFactory
    {
        public static FineTicket CreateFineTicket(int memberID, string reason, decimal amount)
        {
            return new FineTicket(memberID, reason, amount);
        }
    }

    // UI
    // Command Pattern Interface
    public interface ICommand
    {
        void Execute();
    }

    // Concrete Commands
    public class FindMemberCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public FindMemberCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }

        public void Execute()
        {
            Console.Write("Nhập ID thành viên: ");
            int ID = int.Parse(Console.ReadLine());
            Member member = library.FindMemberByID(ID);
            if (member != null)
            {
                Console.WriteLine($"ID: {member.Id}, Tên: {member.Name}, Ngày sinh: {member.DateOfBirth.ToShortDateString()}");
                Console.WriteLine("Sách đã mượn:");
                foreach (Book book in member.BorrowedBooks)
                {
                    Console.WriteLine($"- {book.Title}");
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy thành viên.");
            }
        }
    }
    public class FindBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public FindBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }

        public void Execute()
        {
            Console.WriteLine("\n=== TÌM KIẾM SÁCH ===");
            Console.Write("Nhập tiêu đề sách: ");
            string title = Console.ReadLine();
            List<Book> books = library.FindBookByTitle(title);
            if (books.Count>=1)
            {
                    // Print the header
                    Console.WriteLine($"{"ID",-10}| {"Tiêu đề",-30}| {"Tác giả",-20}| {"Nhà xuất bản",-20}| {"Số lượng",-10}");
                    Console.WriteLine(new string('-', 100)); // Separator line

                    foreach (Book book in books)
                    {
                        Console.WriteLine($"{book.Id,-10}| {book.Title,-30}| {book.Author,-20}| {book.Publisher,-20}| {book.Quantity,-10}");
                    }
            }
            else
            {
                Console.WriteLine("Không tìm thấy sách.");
            }
        }
    }

    public class AddMemberCommand : ICommand 
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public AddMemberCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }

        public void Execute()
        {
            Console.WriteLine("\n=== THÊM THÀNH VIÊN MỚI ===");
            Console.Write("Nhập tên thành viên: ");
            string name = Console.ReadLine();

            DateTime dateOfBirth;
            while (true)
            {
                Console.Write("Nhập ngày sinh (dd/MM/yyyy): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dateOfBirth))
                    break;
                Console.WriteLine("Định dạng ngày không hợp lệ. Vui lòng thử lại!");
            }

            Member newMember = new Member(name, dateOfBirth);
            library.AddMember(newMember);
            Console.WriteLine("Thêm thành viên thành công!");
        }
    }
    public class EditMemberCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public EditMemberCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== CHỈNH SỬA THÔNG TIN THÀNH VIÊN ===");
            Console.Write("Nhập ID thành viên: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID không hợp lệ!");
                return;
            }

            Member member = library.FindMemberByID(id);
            if (member == null)
            {
                Console.WriteLine("Không tìm thấy thành viên!");
                return;
            }

            Console.Write("Nhập tên mới (Enter để giữ nguyên): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                member.Name = newName;

            Console.Write("Nhập ngày sinh mới (dd/MM/yyyy) (Enter để giữ nguyên): ");
            string newDateStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDateStr))
            {
                if (DateTime.TryParseExact(newDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime newDate))
                    member.DateOfBirth = newDate;
                else
                    Console.WriteLine("Định dạng ngày không hợp lệ. Giữ nguyên ngày sinh cũ!");
            }

            library.UpdateMember(member);
            Console.WriteLine("Cập nhật thông tin thành công!");
        }
    }
    public class DisplayMemberCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public DisplayMemberCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== DANH SÁCH THÀNH VIÊN ===");
            List<Member> members = library.GetAllMembers();
            if (members.Count == 0)
            {
                Console.WriteLine("Chưa có thành viên nào!");
                return;
            }
            Console.WriteLine($"{"ID",-10}| {"Tên",-30}| {"Ngày sinh",-15}| {"Số sách đang mượn",-20}");
            Console.WriteLine(new string('-', 75)); // Separator line

            foreach (Member member in members)
            {
                Console.WriteLine($"{member.Id,-10}| {member.Name,-30}| {member.DateOfBirth.ToShortDateString(),-15}| {member.BorrowedBooks.Count,-30}");
            }
        }
    }
    public class GetBorrowedBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public GetBorrowedBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== DANH SÁCH SÁCH MƯỢN  ===");
            Console.Write("Nhập ID thành viên: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID không hợp lệ!");
                return;
            }
            Member member = library.FindMemberByID(id);
            List<Book> books = library.GetAllBorrowBook(member);
            if (books.Count == 0)
                Console.WriteLine("Chưa mượn sách nào");
            Console.WriteLine($"{"ID",-10}| {"Tiêu đề",-30}| {"Tác giả",-20}| {"Nhà xuất bản",-20}|");
            Console.WriteLine(new string('-', 100)); // Separator line

            foreach (Book book in books)
            {
                Console.WriteLine($"{book.Id,-10}| {book.Title,-30}| {book.Author,-20}| {book.Publisher,-20}|");
            }
            Console.WriteLine($"Mượn tổng cộng {books.Count} sách");
        }
    }
    public class AddBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public AddBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== THÊM SÁCH MỚI ===");
            Console.Write("Nhập tiêu đề: ");
            string title = Console.ReadLine();
            Console.Write("Nhập tác giả: ");
            string author = Console.ReadLine();
            Console.Write("Nhập nhà xuất bản: ");
            string publisher = Console.ReadLine();
            Console.Write("Nhập số lượng: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity))
            {
                Console.WriteLine("Số lượng không hợp lệ!");
                return;
            }

            Book newBook = new Book(title, author, publisher, quantity);
            library.AddBook(newBook);
            Console.WriteLine("Thêm sách thành công!");
        }
    }
    public class EditBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public EditBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== CHỈNH SỬA THÔNG TIN SÁCH ===");
            Console.Write("Nhập ID sách: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID không hợp lệ!");
                return;
            }

            Book book = library.FindBookByID(id);

            if (book == null)
            {
                Console.WriteLine("Không tìm thấy sách!");
                return;
            }

            Console.Write("Nhập tiêu đề mới (Enter để giữ nguyên): ");
            string newTitle = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newTitle))
                book.Title = newTitle;

            Console.Write("Nhập tác giả mới (Enter để giữ nguyên): ");
            string newAuthor = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newAuthor))
                book.Author = newAuthor;

            Console.Write("Nhập nhà xuất bản mới (Enter để giữ nguyên): ");
            string newPublisher = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPublisher))
                book.Publisher = newPublisher;

            Console.Write("Nhập số lượng mới (Enter để giữ nguyên): ");
            string newQuantityStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newQuantityStr))
            {
                if (int.TryParse(newQuantityStr, out int newQuantity))
                    book.Quantity = newQuantity;
                else
                    Console.WriteLine("Số lượng không hợp lệ. Giữ nguyên số lượng cũ!");
            }

            library.UpdateBook(book);
            Console.WriteLine("Cập nhật thông tin sách thành công!");
        }
    }
    public class RemoveBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public RemoveBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== XÓA SÁCH ===");
            Console.Write("Nhập ID sách cần xóa: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID không hợp lệ!");
                return;
            }

            library.RemoveBook(id);
            Console.WriteLine("Xóa sách thành công!");
        }
    }
    public class DisplayBookCommand: ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public DisplayBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== DANH SÁCH SÁCH ===");
            List<Book> books = library.GetAllBooks();
            if (books.Count == 0)
            {
                Console.WriteLine("Chưa có sách nào!");
                return;
            }

            // Print the header
            Console.WriteLine($"{"ID",-10}| {"Tiêu đề",-30}| {"Tác giả",-20}| {"Nhà xuất bản",-20}| {"Số lượng",-10}| {"Tình trạng",-10}");
            Console.WriteLine(new string('-', 100)); // Separator line
            foreach (Book book in books)
            {
                Console.WriteLine($"{book.Id,-10}| {book.Title,-30}| {book.Author,-20}| {book.Publisher,-20}| {book.Quantity,-10}| {book.IsAvailable,-10}");
            }
        }
    }
    public class BorrowBookCommand: ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public BorrowBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== MƯỢN SÁCH ===");
            Console.Write("Nhập ID thành viên: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("ID thành viên không hợp lệ!");
                return;
            }

            Member member = library.FindMemberByID(memberId);
            if (member == null)
            {
                Console.WriteLine("Không tìm thấy thành viên!");
                return;
            }

            Console.Write("Nhập ID sách: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("ID sách không hợp lệ!");
                return;
            }
            Book book = library.FindBookByID(bookId);
            if (book == null)
            {
                Console.WriteLine("Không tìm thấy sách!");
                return;
            }

            if (library.BorrowBook(member, book))
            {
                Console.WriteLine("Mượn sách thành công!");
            }
            else
            {
                Console.WriteLine("Không thể mượn sách. Có thể sách đã hết hoặc thành viên đã mượn quá số lượng cho phép.");
            }
        }
    }
    public class ReturnBookCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public ReturnBookCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.WriteLine("\n=== TRẢ SÁCH ===");
            Console.Write("Nhập ID thành viên: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("ID thành viên không hợp lệ!");
                return;
            }

            Member member = library.FindMemberByID(memberId);
            if (member == null)
            {
                Console.WriteLine("Không tìm thấy thành viên!");
                return;
            }

            Console.Write("Nhập ID sách: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("ID sách không hợp lệ!");
                return;
            }

            Book book = library.FindBookByID(bookId);
            if (book == null)
            {
                Console.WriteLine("Không tìm thấy sách!");
                return;
            }

            if (library.ReturnBook(member, book))
            {
                Console.WriteLine("Trả sách thành công!");
            }
            else
            {
                Console.WriteLine("Không thể trả sách. Có thể thành viên không mượn sách này.");
            }
        }
    }
    public class DisplayFineCommand: ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public DisplayFineCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            List<FineTicket> fines = library.GetAllFines();
            if (fines.Count == 0)
            {
                Console.WriteLine("Không có khoản phạt nào.");
                return;
            }

            Console.WriteLine("Danh sách các khoản phạt:");
            // Print the header
            Console.WriteLine($"{"ID",-10}| {"ID Thành viên",-15}| {"Lý do",-30}| {"Số tiền",-10}| {"Đã trả",-10}| {"Ngày phạt",-15}");
            Console.WriteLine(new string('-', 100)); // Separator line
            foreach (FineTicket fine in fines)
            {
                Console.WriteLine($"{fine.Id,-10}| {fine.MemberID,-15}| {fine.Reason,-30}| {fine.Amount,-10}| {(fine.IsPaid ? "Có" : "Không"),-10}| {fine.IssuedDate.ToShortDateString(),-15}");
            }
        }
    }
    public class AddFineCommand : ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public AddFineCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.Write("Nhập ID thành viên bị phạt: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("ID thành viên không hợp lệ!");
                return;
            }

            Member member = library.FindMemberByID(memberId);
            if (member == null)
            {
                Console.WriteLine("Không tìm thấy thành viên với ID này.");
                return;
            }

            Console.Write("Nhập lý do phạt: ");
            string reason = Console.ReadLine();

            Console.Write("Nhập số tiền phạt: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Số tiền không hợp lệ!");
                return;
            }

            FineTicket fine = FineTicketFactory.CreateFineTicket(member.Id, reason, amount);
            library.AddFine(fine, member.Id);
            Console.WriteLine("Đã thêm khoản phạt thành công.");
        }
    }
    public class PayFineCommand:ICommand
    {
        private readonly LibraryUI ui;
        private readonly Library library;

        public PayFineCommand(LibraryUI ui, Library library)
        {
            this.ui = ui;
            this.library = library;
        }
        public void Execute()
        {
            Console.Write("Nhập ID khoản phạt cần thanh toán: ");
            if (!int.TryParse(Console.ReadLine(), out int fineId))
            {
                Console.WriteLine("ID khoản phạt không hợp lệ!");
                return;
            }

            FineTicket fine = library.FindFineTicket(fineId);
            if (fine == null)
            {
                Console.WriteLine("Không tìm thấy khoản phạt với ID này.");
                return;
            }
            library.PayFine(fine);
            if (fine.IsPaid)
            {
                Console.WriteLine("Khoản phạt đã được thanh toán.");
            }
            else
            {
                Console.WriteLine("Không thể thanh toán khoản phạt. Có thể khoản phạt đã được thanh toán trước đó.");
            }
        }
    }

    // UI Class with Command Pattern implementation
    public class LibraryUI
    {
        private readonly Library library;
        private readonly Dictionary<string, ICommand> commands;

        public LibraryUI()
        {
            library = Library.GetInstance();
            commands = new Dictionary<string, ICommand>();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            commands["findmember"] = new FindMemberCommand(this, library);
            commands["findbook"] = new FindBookCommand(this, library);
            commands["addmember"] = new AddMemberCommand(this, library);
            commands["editmember"] = new EditMemberCommand(this, library);
            commands["displaymember"] = new DisplayMemberCommand(this, library);
            commands["getborrowedbook"] = new GetBorrowedBookCommand(this, library);
            commands["addbook"] = new AddBookCommand(this, library);
            commands["editbook"] = new EditBookCommand(this, library);
            commands["removebook"] = new RemoveBookCommand(this, library);
            commands["displaybook"] = new DisplayBookCommand(this, library);
            commands["borrowbook"] = new BorrowBookCommand(this, library);
            commands["returnbook"] = new ReturnBookCommand(this, library);
            commands["displayfine"] = new DisplayFineCommand(this, library);
            commands["addfine"] = new AddFineCommand(this, library);
            commands["payfine"] = new PayFineCommand(this, library);
            // Add other commands here
        }

        public void Run()
        {
            Manager loggedInManager = Login();

            if (loggedInManager != null)
            {
                MainMenu();
            }
            else
            {
                Console.WriteLine("Đăng nhập không thành công.");
            }
        }

        private Manager Login()
        {
            Console.WriteLine("=== ĐĂNG NHẬP HỆ THỐNG QUẢN LÝ THƯ VIỆN ===");
            Console.Write("Tài khoản: ");
            string username = Console.ReadLine();
            Console.Write("Mật khẩu: ");
            string password = Console.ReadLine();

            return library.Login(username, password);
        }

        private void MainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MENU CHÍNH ===");
                Console.WriteLine("1. Quản lý Thành viên");
                Console.WriteLine("2. Quản lý Sách");
                Console.WriteLine("3. Quản lý Phạt");
                Console.WriteLine("4. Thoát");
                Console.Write("Chọn chức năng (1-4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MemberMenu();
                        break;
                    case "2":
                        BookMenu();
                        break;
                    case "3":
                        FineMenu();
                        break;
                    case "4":
                        Console.WriteLine("Cảm ơn bạn đã sử dụng hệ thống!");
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }
            }
        }

        private void MemberMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== QUẢN LÝ THÀNH VIÊN ===");
                Console.WriteLine("1. Tìm kiếm thành viên");
                Console.WriteLine("2. Thêm thành viên mới");
                Console.WriteLine("3. Chỉnh sửa thông tin thành viên");
                Console.WriteLine("4. Xem danh sách thành viên");
                Console.WriteLine("5. Xem sách đang mượn của thành viên");
                Console.WriteLine("6. Quay lại menu chính");
                Console.Write("Chọn chức năng (1-5): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        commands["findmember"].Execute();
                        break;
                    case "2":
                        commands["addmember"].Execute();
                        break;
                    case "3":
                        commands["editmember"].Execute();
                        break;
                    case "4":
                        commands["displaymember"].Execute();
                        break;
                    case "5":
                        commands["getborrowedbook"].Execute();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }
                Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
                Console.ReadKey();
            }
        }

        private void BookMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== QUẢN LÝ SÁCH ===");
                Console.WriteLine("1. Tìm kiếm sách");
                Console.WriteLine("2. Thêm sách mới");
                Console.WriteLine("3. Chỉnh sửa thông tin sách");
                Console.WriteLine("4. Xóa sách");
                Console.WriteLine("5. Xem danh sách sách");
                Console.WriteLine("6. Mượn sách");
                Console.WriteLine("7. Trả sách");
                Console.WriteLine("8. Quay lại menu chính");
                Console.Write("Chọn chức năng (1-8): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        commands["findbook"].Execute();
                        break;
                    case "2":
                        commands["addbook"].Execute();
                        break;
                    case "3":
                        commands["editbook"].Execute();
                        break;
                    case "4":
                        commands["removebook"].Execute();
                        break;
                    case "5":
                        commands["displaybook"].Execute();
                        break;
                    case "6":
                        commands["displaybook"].Execute();
                        commands["borrowbook"].Execute();
                        break;
                    case "7":
                        commands["getborrowedbook"].Execute();
                        commands["returnbook"].Execute();
                        break;
                    case "8":
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }
                Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
                Console.ReadKey();
            }
        }

        private void FineMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== QUẢN LÝ PHẠT ===");
                Console.WriteLine("1. Xem danh sách phạt");
                Console.WriteLine("2. Thêm khoản phạt");
                Console.WriteLine("3. Thanh toán khoản phạt");
                Console.WriteLine("4. Quay lại menu chính");
                Console.Write("Chọn chức năng (1-4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        commands["displayfine"].Execute();
                        break;
                    case "2":
                        commands["addfine"].Execute();
                        break;
                    case "3":
                        commands["payfine"].Execute();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }
                Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
                Console.ReadKey();
            }
        }

    
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            LibraryUI ui = new LibraryUI();
            ui.Run();
        }
    }
}