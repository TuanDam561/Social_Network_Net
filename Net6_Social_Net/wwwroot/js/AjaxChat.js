
    document.addEventListener("DOMContentLoaded", function () {
                const friendCards = document.querySelectorAll(".Card-friend");
    const messenger = document.querySelector(".messenger");
    const userNameDiv = document.querySelector(".UserName");

                friendCards.forEach((card) => {
        card.addEventListener("click", function () {
            const friendId = this.getAttribute("data-friend-id");

            // Gửi yêu cầu AJAX để lấy tin nhắn của người dùng này
            fetch(`/Messenger/GetMessages?friendId=${friendId}`)
                .then((response) => response.json())
                .then((data) => {
                    // Hiển thị tên người dùng trong phần UserName
                    const friendName = data.length > 0 ? data[0].friendName : this.getAttribute("data-friend-name");
                    userNameDiv.textContent = friendName || "Người dùng";

                    // Xóa nội dung cũ trong messenger
                    messenger.innerHTML = "";

                    // Duyệt qua danh sách tin nhắn và hiển thị
                    data.forEach((message) => {
                        const messageElement = document.createElement("div");
                        messageElement.classList.add(message.isSender ? "MyMess" : "friendMess");
                        messageElement.setAttribute("data-messenger-id", message.messengerId); // Gắn MessengerId

                        if (message.isSender) {
                            // Tạo nút xóa tin nhắn
                            const deleteButton = document.createElement("span");
                            deleteButton.classList.add("deletemymess");
                            deleteButton.textContent = "...";
                            deleteButton.style.margin = "7px";
                            deleteButton.style.cursor = "pointer";

                            deleteButton.addEventListener("click", function () {
                                const messengerId = messageElement.getAttribute("data-messenger-id");

                                // if (confirm("Bạn có chắc chắn muốn xóa tin nhắn này?")) {
                                //     Gọi SignalR để xóa tin nhắn
                                //     connection.invoke("DeleteMessage", parseInt(messengerId))
                                //         .catch(err => console.error("Error deleting message:", err));
                                // }
                            });

                            messageElement.appendChild(deleteButton);
                        } else {
                            // Tạo avatar cho tin nhắn của người nhận
                            const avatar = document.createElement("img");
                            avatar.src = message.avatar || "/Avatar/454849238_800890952251701_8841246141296174912_n.jpg";
                            avatar.alt = "Avatar";
                            messageElement.appendChild(avatar);
                        }

                        // Tạo nội dung tin nhắn
                        const chat = document.createElement("div");
                        chat.textContent = message.content;
                        chat.classList.add(message.isSender ? "myChat" : "friendChat");

                        messageElement.appendChild(chat);
                        messenger.appendChild(messageElement);
                    });

                    // Cuộn tới cuối cùng
                    messenger.scrollTop = messenger.scrollHeight;
                })
                .catch(err => console.error("Error fetching messages:", err));
        });
                });
            });


