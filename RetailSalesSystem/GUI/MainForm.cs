using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RetailSalesSystem.BUS;
using RetailSalesSystem.DTO;

namespace RetailSalesSystem.GUI
{
    public class MainForm : Form
    {
        DataGridView dgvSanPham, dgvGioHang;
        ComboBox cbKhachHang;
        TextBox txtSoLuong, txtTimKiem;
        Button btnThemGioHang, btnThanhToan, btnBaoCao;
        Label lblTongTien;

        SanPhamBUS spBUS = new SanPhamBUS();
        KhachHangBUS khBUS = new KhachHangBUS();
        HoaDonBUS hdBUS = new HoaDonBUS();

        List<ChiTietHDDTO> gioHang = new List<ChiTietHDDTO>();

        public MainForm()
        {
            this.Text = "Hệ Thống Quản Lý Bán Hàng & Siêu Thị Mini";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
            LoadData();
        }

        private void InitializeCustomComponents()
        {
            Label lblSearch = new Label() { Text = "Tìm sản phẩm (Tên/Mã):", Location = new Point(12, 15), AutoSize = true };
            txtTimKiem = new TextBox() { Location = new Point(170, 12), Width = 200 };
            txtTimKiem.TextChanged += (s, e) => { dgvSanPham.DataSource = spBUS.Search(txtTimKiem.Text); };

            dgvSanPham = new DataGridView() { Location = new Point(12, 45), Width = 460, Height = 250, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };

            Label lblSL = new Label() { Text = "Số lượng:", Location = new Point(12, 310), AutoSize = true };
            txtSoLuong = new TextBox() { Text = "1", Location = new Point(80, 307), Width = 60 };
            btnThemGioHang = new Button() { Text = "Thêm vào giỏ >>>", Location = new Point(150, 305), Width = 120, Height = 25 };
            btnThemGioHang.Click += BtnThemGioHang_Click;

            dgvGioHang = new DataGridView() { Location = new Point(490, 45), Width = 480, Height = 250, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };

            Label lblKH = new Label() { Text = "Khách hàng:", Location = new Point(490, 310), AutoSize = true };
            cbKhachHang = new ComboBox() { Location = new Point(580, 307), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            lblTongTien = new Label() { Text = "Tổng tiền: 0 VNĐ", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(490, 350), AutoSize = true, ForeColor = Color.Red };
            
            btnThanhToan = new Button() { Text = "THANH TOÁN (Transaction)", Location = new Point(490, 390), Width = 200, Height = 40, BackColor = Color.LightGreen };
            btnThanhToan.Click += BtnThanhToan_Click;

            btnBaoCao = new Button() { Text = "Xem Doanh Thu & Top 5", Location = new Point(710, 390), Width = 180, Height = 40, BackColor = Color.LightYellow };
            btnBaoCao.Click += BtnBaoCao_Click;

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtTimKiem);
            this.Controls.Add(dgvSanPham);
            this.Controls.Add(lblSL);
            this.Controls.Add(txtSoLuong);
            this.Controls.Add(btnThemGioHang);
            this.Controls.Add(dgvGioHang);
            this.Controls.Add(lblKH);
            this.Controls.Add(cbKhachHang);
            this.Controls.Add(lblTongTien);
            this.Controls.Add(btnThanhToan);
            this.Controls.Add(btnBaoCao);
        }

        private void LoadData()
        {
            dgvSanPham.DataSource = spBUS.GetAll();
            cbKhachHang.DataSource = khBUS.GetAll();
            cbKhachHang.DisplayMember = "TenKH";
            cbKhachHang.ValueMember = "MaKH";
        }

        private void BtnThemGioHang_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count == 0) return;

            var row = dgvSanPham.SelectedRows[0];
            string maSP = row.Cells["MaSP"].Value.ToString();
            string tenSP = row.Cells["TenSP"].Value.ToString();
            decimal giaBan = Convert.ToDecimal(row.Cells["GiaBan"].Value);
            int tonKho = Convert.ToInt32(row.Cells["SoLuongTon"].Value);

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!");
                return;
            }

            if (soLuong > tonKho)
            {
                MessageBox.Show("Số lượng mua vượt quá số lượng tồn kho!");
                return;
            }

            var existingItem = gioHang.Find(x => x.MaSP == maSP);
            if (existingItem != null)
            {
                existingItem.SoLuong += soLuong;
            }
            else
            {
                gioHang.Add(new ChiTietHDDTO { MaSP = maSP, TenSP = tenSP, SoLuong = soLuong, DonGia = giaBan });
            }

            RefreshGioHangGrid();
        }

        private void RefreshGioHangGrid()
        {
            dgvGioHang.DataSource = null;
            dgvGioHang.DataSource = gioHang;

            decimal tongTien = 0;
            foreach (var item in gioHang) tongTien += item.ThanhTien;

            lblTongTien.Text = $"Tổng tiền: {tongTien:N0} VNĐ";
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (gioHang.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống!");
                return;
            }

            try
            {
                int maNV = 1;
                int maKH = Convert.ToInt32(cbKhachHang.SelectedValue);

                decimal tongTien = 0;
                foreach (var item in gioHang) tongTien += item.ThanhTien;

                bool success = hdBUS.ThanhToan(maNV, maKH, tongTien, gioHang);

                if (success)
                {
                    MessageBox.Show("Thanh toán thành công! Hệ thống đã trừ tồn kho và tích điểm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    gioHang.Clear();
                    RefreshGioHangGrid();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giao dịch thất bại (Đã Rollback dữ liệu): " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData();
            }
        }

        private void BtnBaoCao_Click(object sender, EventArgs e)
        {
            decimal doanhThu = hdBUS.GetDoanhThuHomNay();
            DataTable dtTop5 = hdBUS.GetTop5BestSellers();

            string report = $"=== BÁO CÁO THỐNG KÊ ===\n\n- Doanh thu hôm nay: {doanhThu:N0} VNĐ\n\n- Top sản phẩm bán chạy:\n";
            foreach (DataRow r in dtTop5.Rows)
            {
                report += $"+ {r["TenSP"]}: Đã bán {r["TongBan"]} sản phẩm\n";
            }

            MessageBox.Show(report, "Báo cáo Thống kê", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
