// Lấy giá trị userId và userName từ session trong ASP.NET Core
const userId = "@Context.Session.GetString("UserId")";
const userName = "@Context.Session.GetString("UserName")";


src = "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.16/signalr.min.js" >

   // Khởi tạo kết nối SignalR
   const connection = new signalR.HubConnectionBuilder()
    .withUrl("/commentHub") // Địa chỉ của SignalR Hub
    .build();

// Sự kiện nhận bình luận mới từ SignalR
connection.on("ReceiveComment", function (postId, userName, content, createdAt) {
    const commentsList = document.getElementById("commentsList");

    // Tạo phần tử bình luận mới và thêm vào DOM
    const commentDiv = document.createElement("div");
    commentDiv.classList.add("comment");

    // Tạo HTML cho mỗi bình luận
    commentDiv.innerHTML = `<strong>${userName}:</strong> ${content} <br><span>${new Date(createdAt).toLocaleString()}</span>`;

    // Thêm bình luận vào danh sách bình luận trong modal
    commentsList.appendChild(commentDiv);
    // Cập nhật thẻ ẩn
    const hiddenCommentsContainer = document.getElementById(`comments-${postId}`);
    if (hiddenCommentsContainer) {
        const hiddenCommentDiv = document.createElement("div");
        hiddenCommentDiv.classList.add("comment");
        hiddenCommentDiv.innerHTML = `
                                <span class="comment-user-name">${userName}</span>
                                <p class="comment-content">${content}</p>
                                <p class="comment-created-at">${new Date(createdAt).toLocaleString()}</p>`;
        hiddenCommentsContainer.appendChild(hiddenCommentDiv);
    }
});
let currentPostId = null;
// Hàm mở modal
function openPostModal(buttonElement) {
    // Lấy postId từ thuộc tính data-post-id
    currentPostId = buttonElement.getAttribute("data-post-id");
    const modal = document.getElementById("postModal");

    // Gửi yêu cầu tham gia nhóm
    connection.invoke("JoinPostGroup", parseInt(currentPostId, 10))
        .catch(function (err) {
            console.error("Failed to join group:", err.toString());
        });

    // Lấy dữ liệu từ DOM dựa trên ID bài viết
    const content = document.getElementById(`content-${currentPostId}`).innerText;
    const status = document.getElementById(`status-${currentPostId}`).innerText;
    const imageUrl = document.getElementById(`image-${currentPostId}`)?.src || "";
    const createdAt = document.getElementById(`createdAt-${currentPostId}`)?.innerText || "";
    const userName = document.getElementById(`userName-${currentPostId}`)?.innerText || "Người dùng";

    // Đổ dữ liệu vào modal
    document.getElementById("modalContent").innerText = content;
    document.getElementById("modalStatus").innerText = status;
    document.getElementById("modalImage").src = imageUrl;
    document.getElementById("modalCreatedAt").innerText = createdAt;
    document.getElementById("modalUserName").innerText = userName;

    // Lấy các bình luận của bài viết
    const commentsContainer = document.getElementById(`comments-${currentPostId}`);
    const commentsList = document.getElementById("commentsList");
    commentsList.innerHTML = ""; // Xóa các bình luận cũ

    // Duyệt qua các bình luận và hiển thị trong modal
    if (commentsContainer) {
        const comments = commentsContainer.querySelectorAll(".comment");
        comments.forEach(function (commentElement) {
            const commentUserName = commentElement.querySelector(".comment-user-name").innerText;
            const commentContent = commentElement.querySelector(".comment-content").innerText;
            const commentCreatedAt = commentElement.querySelector(".comment-created-at").innerText;


            // Tạo phần tử bình luận mới
            const commentDiv = document.createElement("div");
            commentDiv.classList.add("comment");

            // Tạo HTML cho mỗi bình luận
            commentDiv.innerHTML = `<strong>${commentUserName}:</strong> ${commentContent} <br><span>${commentCreatedAt}</span>`;

            // Thêm bình luận vào danh sách bình luận trong modal
            commentsList.appendChild(commentDiv);
        });
    }
    // Hiển thị modal
    modal.style.display = "flex";
}

// Hàm đóng modal
function closeModalds() {
    const modal = document.getElementById("postModal");
    modal.style.display = "none";

    if (currentPostId !== null) {
        // Gửi yêu cầu rời nhóm
        connection.invoke("LeavePostGroup", parseInt(currentPostId, 10))
            .catch(function (err) {
                console.error("Failed to leave group:", err.toString());
            });
    }
    // Reset postId
    currentPostId = null;
}

// Hàm gửi bình luận
function submitComment() {
    const commentInput = document.getElementById("commentInput");

    if (commentInput) {
        const commentContent = commentInput.value.trim();
        if (commentContent === "") {
            alert("Vui lòng nhập bình luận!");
            return;
        }
        // Chuyển currentPostId từ chuỗi sang số nguyên
        const postId = parseInt(currentPostId, 10);

        // Kiểm tra nếu postId không hợp lệ
        if (isNaN(postId)) {
            console.error("Invalid postId");
            return;
        }
        // Lấy userId từ session và chuyển đổi thành int
        const userId = parseInt("@Context.Session.GetString("UserId")", 10);

        // Kiểm tra nếu userId hợp lệ
        if (isNaN(userId)) {
            console.error("Invalid userId");
            return;
        }

        //const userId = "@Context.Session.GetString("UserId")"; // Lấy userId từ session
        const createdAt = new Date().toISOString();

        console.log("postId:", postId);
        console.log("userId:", userId);
        console.log("commentContent:", commentContent);
        console.log("createdAt:", createdAt);

        // Gửi bình luận qua SignalR đến Hub
        connection.invoke("SendComment", postId, userId, commentContent, createdAt)
            .catch(function (err) {
                return console.error(err.toString());
            });

        // Xóa giá trị trong ô nhập liệu sau khi gửi
        commentInput.value = "";
    }
}
// Kết nối SignalR khi trang được tải
connection.start().catch(function (err) {
    return console.error(err.toString());
});