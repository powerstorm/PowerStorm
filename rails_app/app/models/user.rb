require 'digest/sha2'

class User < ActiveRecord::Base
  attr_accessor :password_confirmation
  attr_reader :password
  
  after_destroy :ensure_an_admin_remains

  def User.authenticate(name, password)
    if user = find_by_name(name)
      if user.hashed_password == encrypt_password(password, user.salt)
        user
      end
    end
  end
  
  def User.encrypt_password(password, salt)
    Digest::SHA2.hexdigest(password + 'yourmom' + salt)
  end
  
  def password=(password)
    @password = password
    if password.present?
      generate_salt
      self.hashed_password = self.class.encrypt_password(password, salt)
    end
  end
  
  private
  def generate_salt
    self.salt = self.object_id.to_s + rand.to_s
  end
  
  def ensure_an_admin_remains
    if User.count.zero?
      raise "Can't delete last user!"
    end
  end
end
