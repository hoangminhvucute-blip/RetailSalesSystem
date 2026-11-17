using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.DAL
{
    public class DatabaseAccess
    {
        protected string connStr = @"Server=DESKTOP-CMSNBP3;Database=SalesDB;Integrated Security=True;";
    }

    public class SanPhamDAL : DatabaseAccess
    {
        public DataTable GetAll()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SanPham", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable Search(string keyword)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM SanPham WHERE MaSP LIKE @kw OR TenSP LIKE @kw";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public bool Insert(SanPhamDTO sp)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "INSERT INTO SanPham(MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@ma, @ten, @gia, @sl)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ma", sp.MaSP);
                cmd.Parameters.AddWithValue("@ten", sp.TenSP);
                cmd.Parameters.AddWithValue("@gia", sp.GiaBan);
                cmd.Parameters.AddWithValue("@sl", sp.SoLuongTon);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    public class KhachHangDAL : DatabaseAccess
    {
        public DataTable GetAll()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM KhachHang", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }

    public class HoaDonDAL : DatabaseAccess
    {
        public bool ThanhToan(int maNV, int maKH, decimal tongTien, List<ChiTietHDDTO> chiTietList)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    string insertHD = "INSERT INTO HoaDon (MaNV, MaKH, TongTien) OUTPUT INSERTED.MaHD VALUES (@nv, @kh, @tong)";
                    SqlCommand cmdHD = new SqlCommand(insertHD, conn, trans);
                    cmdHD.Parameters.AddWithValue("@nv", maNV);
                    cmdHD.Parameters.AddWithValue("@kh", maKH);
                    cmdHD.Parameters.AddWithValue("@tong", tongTien);

                    int maHD = (int)cmdHD.ExecuteScalar();

                    foreach (var item in chiTietList)
                    {
                        string checkStockQuery = "SELECT SoLuongTon FROM SanPham WHERE MaSP = @masp";
                        SqlCommand cmdCheck = new SqlCommand(checkStockQuery, conn, trans);
                        cmdCheck.Parameters.AddWithValue("@masp", item.MaSP);
                        int tonKho = (int)cmdCheck.ExecuteScalar();

                        if (tonKho < item.SoLuong)
                        {
                            trans.Rollback();
                            throw new Exception($"Sản phẩm {item.TenSP} không đủ số lượng tồn kho!");
                        }

                        string insertCT = "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES (@mhd, @masp, @sl, @gia)";
                        SqlCommand cmdCT = new SqlCommand(insertCT, conn, trans);
                        cmdCT.Parameters.AddWithValue("@mhd", maHD);
                        cmdCT.Parameters.AddWithValue("@masp", item.MaSP);
                        cmdCT.Parameters.AddWithValue("@sl", item.SoLuong);
                        cmdCT.Parameters.AddWithValue("@gia", item.DonGia);
                        cmdCT.ExecuteNonQuery();

                        string updateStock = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl WHERE MaSP = @masp";
                        SqlCommand cmdUpd = new SqlCommand(updateStock, conn, trans);
                        cmdUpd.Parameters.AddWithValue("@sl", item.SoLuong);
                        cmdUpd.Parameters.AddWithValue("@masp", item.MaSP);
                        cmdUpd.ExecuteNonQuery();
                    }

                    int diemCong = (int)(tongTien / 100000);
                    if (diemCong > 0)
                    {
                        string updateDiem = "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @diem WHERE MaKH = @makh";
                        SqlCommand cmdDiem = new SqlCommand(updateDiem, conn, trans);
                        cmdDiem.Parameters.AddWithValue("@diem", diemCong);
                        cmdDiem.Parameters.AddWithValue("@makh", maKH);
                        cmdDiem.ExecuteNonQuery();
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("Lỗi thanh toán: " + ex.Message);
                }
            }
        }

        public decimal GetDoanhThuHomNay()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public DataTable GetTop5BestSellers()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT TOP 5 sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongBan FROM ChiTietHoaDon ct JOIN SanPham sp ON ct.MaSP = sp.MaSP GROUP BY sp.MaSP, sp.TenSP ORDER BY TongBan DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}
