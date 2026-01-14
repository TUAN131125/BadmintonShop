using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace BadmintonShop.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRoleService _roleService;

        public UserService(IUnitOfWork uow, IRoleService roleService)
        {
            _uow = uow;
            _roleService = roleService;
        }

        // ================= BASIC CRUD =================

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _uow.UserRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _uow.UserRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(User user, string password)
        {
            // Hash mật khẩu và thiết lập các giá trị mặc định cho User mới (thường dùng cho Admin tạo user)
            user.PasswordHash = HashPassword(password);
            user.IsActive = true;
            user.IsGuest = false;
            user.CreatedAt = DateTime.UtcNow;

            await _uow.UserRepository.AddAsync(user);
            await _uow.SaveAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _uow.UserRepository.Update(user);
            await _uow.SaveAsync();
        }

        // Khóa tài khoản (Soft Delete hoặc Disable)
        public async Task LockAsync(int id)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _uow.SaveAsync();
            }
        }

        // Mở khóa tài khoản
        public async Task UnlockAsync(int id)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                await _uow.SaveAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAdminsAsync()
        {
            return await _uow.UserRepository.GetAllAsync(u => u.Role.Name == "Admin");
        }

        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            return await _uow.UserRepository.GetAllAsync(u => u.Role.Name == "Customer");
        }

        // ================= GUEST LOGIC =================

        // Tìm hoặc tạo tài khoản Guest (Dùng cho chức năng thanh toán không cần đăng nhập)
        public async Task<User> FindOrCreateAsync(string email, string fullName, string phone)
        {
            var existingUsers = await _uow.UserRepository.GetAllAsync(u => u.Email == email);
            var existingUser = existingUsers.FirstOrDefault();

            if (existingUser != null)
                return existingUser;

            var user = new User
            {
                Email = email,
                FullName = fullName,
                Phone = phone,
                IsGuest = true, // Đánh dấu là khách vãng lai
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.UserRepository.AddAsync(user);
            await _uow.SaveAsync();
            return user;
        }

        // ================= AUTHENTICATION & REGISTRATION =================

        // Đăng ký tài khoản Customer mới (hoặc nâng cấp từ Guest lên Customer)
        public async Task<User> RegisterCustomerAsync(string fullName, string email, string phone, string address, string province, string password)
        {
            var existingUsers = await _uow.UserRepository.GetAllAsync(u => u.Email == email);
            var existingUser = existingUsers.FirstOrDefault();

            // 1. Đảm bảo Role "Customer" tồn tại trong DB
            var customerRole = await _roleService.GetByNameAsync("Customer");
            if (customerRole == null)
            {
                // Nếu chưa có thì tạo mới (đề phòng DB trống)
                customerRole = new Role { Name = "Customer", IsSystemRole = false };
                await _uow.RoleRepository.AddAsync(customerRole);
                await _uow.SaveAsync();

                // Lấy lại Role vừa tạo để có ID
                customerRole = await _roleService.GetByNameAsync("Customer");
            }

            User userToSave;

            if (existingUser != null)
            {
                // Nếu Email đã tồn tại và KHÔNG phải là Guest -> Báo lỗi trùng Email
                if (!existingUser.IsGuest)
                    throw new Exception("Email is already registered.");

                // Nếu là Guest cũ -> Cập nhật thông tin thành Member chính thức
                userToSave = existingUser;
                userToSave.IsGuest = false;
                userToSave.UpdatedAt = DateTime.UtcNow;

                userToSave.Username = email; // Customer dùng Email làm Username
                userToSave.FullName = fullName;
                userToSave.Phone = phone;
                userToSave.Address = address;
                userToSave.Province = province;
                userToSave.Country = "Vietnam";

                userToSave.RoleId = customerRole.Id;
                userToSave.PasswordHash = HashPassword(password);

                _uow.UserRepository.Update(userToSave);
            }
            else
            {
                // Tạo mới hoàn toàn
                userToSave = new User
                {
                    Username = email,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Address = address,
                    Province = province,
                    Country = "Vietnam",

                    IsActive = true,
                    IsGuest = false,
                    CreatedAt = DateTime.UtcNow,
                    RoleId = customerRole.Id,
                    PasswordHash = HashPassword(password)
                };

                await _uow.UserRepository.AddAsync(userToSave);
            }

            await _uow.SaveAsync();
            return userToSave;
        }

        // Xác thực đăng nhập (Login)
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            // 1. Tìm user theo email
            var users = await _uow.UserRepository.GetAllAsync(u => u.Email == email);
            var user = users.FirstOrDefault();

            // 2. Kiểm tra tài khoản tồn tại và đang hoạt động (Active)
            if (user == null || !user.IsActive) return null;

            // --- QUAN TRỌNG: NẠP ROLE ---
            // Vì Repository mặc định có thể không Include Role, ta phải nạp thủ công
            // để đảm bảo Claims khi Login có chứa Role (Admin/Customer)
            if (user.Role == null && user.RoleId > 0)
            {
                user.Role = await _uow.RoleRepository.GetByIdAsync(user.RoleId);
            }

            // 3. Kiểm tra mật khẩu (Hash)
            var inputHash = HashPassword(password);
            if (user.PasswordHash != inputHash) return null;

            // 4. Cập nhật thời gian đăng nhập lần cuối
            user.LastLoginAt = DateTime.UtcNow;
            await _uow.SaveAsync();

            return user;
        }

        // Helper: Hash Password dùng SHA256
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}