using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Midgard.DbModels
{
    public class MidgardContext : DbContext
    {
        public MidgardContext(DbContextOptions<MidgardContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Skin> Skins { get; set; }
        public virtual DbSet<Cape> Capes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasComment("用户信息表");
            builder.Entity<User>().Property(u => u.Id).HasComment("用户ID");
            builder.Entity<User>().Property(u => u.Username).HasComment("用户名");
            builder.Entity<User>().Property(u => u.Email).HasComment("邮箱");
            builder.Entity<User>().Property(u => u.Password).HasComment("密码");
            builder.Entity<User>().Property(u => u.PasswordSalt).HasComment("密码盐");
            builder.Entity<User>().Property(u => u.IsEmailVerified).HasComment("是否已经验证邮箱");
            builder.Entity<User>().Property(u => u.Permission).HasComment("权限");
            builder.Entity<User>().Property(u => u.TryTimes).HasComment("自上次登录成功后的尝试次数");
            builder.Entity<User>().Property(u => u.CoolDownLevel).HasComment("冷却等级");
            builder.Entity<User>().Property(u => u.CoolDownEndTime).HasComment("冷却结束时间");
            builder.Entity<User>()
                .HasMany(u => u.Profiles)
                .WithOne(p => p.Owner);
            builder.Entity<User>()
                .HasMany(u => u.Tokens)
                .WithOne(t => t.BindUser);

            builder.Entity<Profile>().HasComment("角色信息表");
            builder.Entity<Profile>().Property(p => p.Id).HasComment("角色ID");
            builder.Entity<Profile>().Property(p => p.Name).HasComment("角色名");
            builder.Entity<Profile>().Property(p => p.IsSelected).HasComment("是否已选择");

            builder.Entity<Token>().HasComment("令牌信息表");
            builder.Entity<Token>().HasKey(t => new {t.AccessToken, t.ClientToken});
            builder.Entity<Token>().Property(t => t.AccessToken).HasComment("访问令牌");
            builder.Entity<Token>().Property(t => t.ClientToken).HasComment("客户端令牌");
            builder.Entity<Token>().Property(t => t.IssueTime).HasComment("颁发时间");
            builder.Entity<Token>().Property(t => t.ExpireTime).HasComment("过期时间");
            builder.Entity<Token>().Property(t => t.Status).HasComment("状态");
            builder.Entity<Token>()
                .HasOne(t => t.BindUser)
                .WithMany(u => u.Tokens);
            builder.Entity<Token>()
                .HasOne(t => t.BindProfile)
                .WithMany(p => p.BindTokens);

            builder.Entity<Session>().HasComment("会话信息表");
            builder.Entity<Session>().HasKey(s => new {s.AccessToken, s.ServerId});
            builder.Entity<Session>().Property(s => s.AccessToken).HasComment("访问令牌");
            builder.Entity<Session>().Property(s => s.ServerId).HasComment("服务器ID");
            builder.Entity<Session>().Property(s => s.ClientIp).HasComment("客户端IP")
                .HasConversion(ip => ip.MapToIPv4().ToString(),
                    str => IPAddress.Parse(str));
            builder.Entity<Session>().Property(s => s.ExpireTime).HasComment("过期时间");
            builder.Entity<Session>()
                .HasOne(s => s.BindProfile)
                .WithMany(p => p.ActiveSessions);
            
            builder.Entity<Skin>().HasComment("皮肤信息表");
            builder.Entity<Skin>().Property(s => s.Id).HasComment("皮肤ID");
            builder.Entity<Skin>().Property(s => s.Name).HasComment("皮肤名");
            builder.Entity<Skin>().Property(s => s.Model).HasComment("皮肤模型");
            builder.Entity<Skin>().Property(s => s.Url).HasComment("皮肤地址");
            builder.Entity<Skin>()
                .HasOne(s => s.Owner)
                .WithMany(u => u.Skins);
            
            builder.Entity<Cape>().HasComment("披风信息表");
            builder.Entity<Cape>().Property(c => c.Id).HasComment("披风ID");
            builder.Entity<Cape>().Property(c => c.Name).HasComment("披风名");
            builder.Entity<Cape>().Property(c => c.Url).HasComment("披风地址");
            builder.Entity<Cape>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.Capes);
        }
    }
}