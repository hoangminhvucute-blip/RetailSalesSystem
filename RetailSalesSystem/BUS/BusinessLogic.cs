using System.Collections.Generic;
using System.Data;
using RetailSalesSystem.DAL;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.BUS
{
    public class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();
        public DataTable GetAll() => dal.GetAll();
        public DataTable Search(string kw) => dal.Search(kw);
        public bool Insert(SanPhamDTO sp) => dal.Insert(sp);
    }

    public class KhachHangBUS
    {
        KhachHangDAL dal = new KhachHangDAL();
        public DataTable GetAll() => dal.GetAll();
    }

    public class HoaDonBUS
    {
        HoaDonDAL dal = new HoaDonDAL();
        public bool ThanhToan(int maNV, int maKH, decimal tongTien, List<ChiTietHDDTO> chiTietList)
        {
            return dal.ThanhToan(maNV, maKH, tongTien, chiTietList);
        }
        public decimal GetDoanhThuHomNay() => dal.GetDoanhThuHomNay();
        public DataTable GetTop5BestSellers() => dal.GetTop5BestSellers();
    }
}
