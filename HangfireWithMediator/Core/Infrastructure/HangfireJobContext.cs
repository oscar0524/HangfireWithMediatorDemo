using System;
using Microsoft.EntityFrameworkCore;

namespace HangfireWithMediator.Infrastructure;

public class HangfireJobContext : DbContext
{
    public HangfireJobContext(DbContextOptions<HangfireJobContext> options)
                : base(options)
    {
    }
}
