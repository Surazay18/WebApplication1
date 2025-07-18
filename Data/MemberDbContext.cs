﻿using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class MemberDbContext(DbContextOptions<MemberDbContext> options) : DbContext(options)
    {
        public required DbSet<Member> Members { get; set; }
    }
}