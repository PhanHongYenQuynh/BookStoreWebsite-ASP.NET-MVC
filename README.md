# BookStoreWebsite-ASP.NET-MVC

Tên Website: BookGrotto
Giải nghĩa: Grotto là động phong nha :)))

 



-----------CÁCH SỬ DỤNG GITHUB--------------------------------------

THỰC HIỆN CÁC BƯỚC NHƯ SAU ĐỂ KHÔNG BỊ MẤT CODE VÀ PULL VỀ THÀNH CÔNG: 

---**Trước khi làm:**  

- git pull origin main hoặc git pull

----**Bắt đầu làm :**

1.  git checkout -b “tên nhánh”, 

----**Sau khi làm xong:**

  2.  git branch: kiểm tra nhánh

1.  git status: kiểm tra trạng thái ( đỏ là chưa add, xanh là add rồi ) ( bắt buộc )
2. git add . 
3. git status ( bước có hoặc ko ) ( chuyển xanh trạng thái xanh )
4. git commit -m "nội dung thông báo " 
5. git push origin "tên nhánh "
6. Merger nhánh vào main
7. Xoá nhánh ( bước bắt buộc )
8. Sau khi xoá nhánh  thực hiện như sau :

Mở git bash

git checkout main : lệnh chuyển nhánh main

Sau khi hiện nhánh main rồi mới 

git pull or git pull origin main ( nếu  mới push thì ko cần pull về , qua ngày mới hoặc chức năng mới từ người khác r mới pull )

Sau đó ta thực hiện lại các bước tiếp theo như sau:

Tạo nhánh mới , rồi làm theo cách bước trên , cứ thế vòng lặp vô tận , không xài lại nhánh đã merge r ( vì nhánh đó đã bị xoá vật lý , mặc dù trên local còn nhưng không thể thực hiện chức năng đc )

- —**Lưu ý:** PUSH XONG RỒI **MERGE NGAY** [ không làm theo mất code ]
- —**Lưu ý 2:** MERGE XONG RỒI FAI XOÁ NHÁNH [ không làm theo ko pull về main đc]
- —**Lưu ý 3:** KHÔNG ĐƯỢC PUSH TRÊN HÀM MAIN , FAI TẠO NHÁNH MỚI ĐC PUSH [ gây xung đột code ]
- —**Lưu ý 4:** siêu quang trọng,
1.  pull về main: luôn là bước đầu tiên ,
2.  tạo nhánh: bước quan trọng thứ hai , 
3. Push theo nhánh chỉ định: bước bắt buộc

<aside>
⚠️ **NẾU BỊ LỖI GIỐNG VẬY TA THỰC HIỆN NHƯ SAU:**
Please Commit your change or Stash before your merge
Aborting....
Updating
</aside>

git checkout “ về nhánh phụ “

git add . 

git commit -m “nội dung thông báo “

git checkout main

git pull origin main


----------------------Cách dùng Migration------------------------------
Lỡ tay xoá CreaateDatabase trong migration thì làm sao
Đây nè: Update-Database -TargetMigration "CreatedDatabase" 

