using System.Data;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

internal sealed class ToolIdHandler : SqlMapper.TypeHandler<ToolId>
{
    public override ToolId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, ToolId? value) => parameter.Value = value?.Value;
}

internal sealed class MemberIdHandler : SqlMapper.TypeHandler<MemberId>
{
    public override MemberId Parse(object value) => value is DBNull ? null! : new MemberId((int)value);
    public override void SetValue(IDbDataParameter parameter, MemberId? value) => parameter.Value = value?.Value ?? (object)DBNull.Value;
}

internal sealed class InventoryItemIdHandler : SqlMapper.TypeHandler<InventoryItemId>
{
    public override InventoryItemId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, InventoryItemId? value) => parameter.Value = value?.Value;
}

internal sealed class ReservationIdHandler : SqlMapper.TypeHandler<ReservationId>
{
    public override ReservationId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, ReservationId? value) => parameter.Value = value?.Value;
}

internal sealed class MaintenanceRecordIdHandler : SqlMapper.TypeHandler<MaintenanceRecordId>
{
    public override MaintenanceRecordId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, MaintenanceRecordId? value) => parameter.Value = value?.Value;
}

internal sealed class ManufcaturerIdHandler : SqlMapper.TypeHandler<ManufcaturerId>
{
    public override ManufcaturerId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, ManufcaturerId? value) => parameter.Value = value?.Value;
}

internal sealed class LocationIdHandler : SqlMapper.TypeHandler<LocationId>
{
    public override LocationId Parse(object value) => new((int)value);
    public override void SetValue(IDbDataParameter parameter, LocationId? value) => parameter.Value = value?.Value;
}

public static class TypeHandlerRegistrations
{
    private static bool _registered;
    private static readonly object Lock = new();

    public static void EnsureRegistered()
    {
        if (_registered) return;
        lock (Lock)
        {
            if (_registered) return;
            SqlMapper.AddTypeHandler(new ToolIdHandler());
            SqlMapper.AddTypeHandler(new MemberIdHandler());
            SqlMapper.AddTypeHandler(new InventoryItemIdHandler());
            SqlMapper.AddTypeHandler(new ReservationIdHandler());
            SqlMapper.AddTypeHandler(new MaintenanceRecordIdHandler());
            SqlMapper.AddTypeHandler(new ManufcaturerIdHandler());
            SqlMapper.AddTypeHandler(new LocationIdHandler());
            _registered = true;
        }
    }
}
