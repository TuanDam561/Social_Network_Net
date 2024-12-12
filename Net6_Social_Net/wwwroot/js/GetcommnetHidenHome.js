   /*   let currentPostId = null;
                        // Hàm mở modal
                          function openPostModal(buttonElement) {
                                 document.body.classList.add("modal-open"); // Ngăn cuộn
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
                               // const imageUrl = document.getElementById(`image-${currentPostId}`)?.src || "";

                                const imageElement = document.getElementById(`image-${currentPostId}`); // Lấy thẻ img
                                const imageUrl = imageElement ? imageElement.src : ""; // Kiểm tra xem thẻ img có tồn tại không

                                const createdAt = document.getElementById(`createdAt-${currentPostId}`)?.innerText || "";
                                // const userName = document.getElementById(`userName-${currentPostId}`)?.innerText || "Người dùng";

                                // Lấy username và userid từ thẻ tương ứng
                                const userNameElement = document.getElementById(`userName-${currentPostId}`);
                                const userName = userNameElement?.innerText || "Người dùng"; // Tên người dùng
                                const commentUserId = userNameElement?.getAttribute("data-userr-id"); // Lấy userId từ thuộc tính data

                                    // Đổ dữ liệu vào modal
                                    document.getElementById("modalContent").innerText = content;
                                    document.getElementById("modalStatus").innerText = status;
                                    //document.getElementById("modalImage").src = imageUrl;
                                    document.getElementById("modalCreatedAt").innerText = createdAt;
                                  //  document.getElementById("modalUserName").innerText = userName;

                                    const modalUserName = document.getElementById("modalUserName");
                                    modalUserName.innerText = userName;
                                    if (commentUserId) {
                                        modalUserName.href = `/UserAccount/Index?id=${commentUserId}`;
                                    } else {
                                        modalUserName.removeAttribute("href"); // Nếu không có userId, xóa href
                                    }

                                   // Xử lý ảnh
                                    const modalImageContainer = document.querySelector(".post-image-container");
                                    const modalImage = document.getElementById("modalImage");

                                    if (imageUrl) {
                                        modalImage.src = imageUrl; // Gán URL hình ảnh
                                        modalImageContainer.style.display = "block"; // Hiển thị container ảnh
                                    } else {
                                        modalImage.src="/Icon_Plafom/454849238_800890952251701_8841246141296174912_n.jpg";
                                        modalImageContainer.style.display = "block"; // Ẩn container nếu không có ảnh
                                    }


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
                                            const commentId = commentElement.getAttribute("data-comment-id");

                                            const commentUserId = commentElement.getAttribute("data-user-id"); // Lấy userId của bình luận
                                    
                                            const postUserId=commentElement.getAttribute("data-userpostsub-id");

                                           const userIdSS = parseInt("@Context.Session.GetString("UserId")", 10);
                                            // Tạo phần tử bình luận mới
                                            const commentDiv = document.createElement("div");
                                            commentDiv.classList.add("comment");
                                            commentDiv.setAttribute("data-comment-id", commentId);  
                                    
                                         const showDeleteButton = userIdSS == commentUserId 
                                         const showDeletUserPost= userIdSS == postUserId;
                                         const showDeleteIcon = showDeleteButton || showDeletUserPost;
                                            // Tạo HTML cho mỗi bình luận
                                         commentDiv.innerHTML =
                                        `<div style="display: flex; justify-content: space-between;">
                                         <a style="text-transform: capitalize; font-weight:bold;font-size:16px; cursor:pointer; text-decoration:none;" href="/UserAccount/Index?id=${commentUserId}">
                                          ${commentUserName}
                                         </a>
                                        ${
                                         showDeleteIcon
                                        ? `<span style="font-size:20px; font-weight:bold;cursor:pointer;" onclick="deleteComment(${commentId})">...</span>`
                                         : ''
                                            }
                                   
                                        </div>
                                        ${commentContent}  
                                        <br>
                                        <span>${commentCreatedAt}</span>
                                        `;
                                            // Thêm bình luận vào danh sách bình luận trong modal
                                            commentsList.appendChild(commentDiv);
                                        });
                                    }
                                    // Hiển thị modal
                                    modal.style.display = "flex";
                                }*/