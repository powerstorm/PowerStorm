class CreateWeathers < ActiveRecord::Migration
  def self.up
    create_table :weathers do |t|
      t.datetime :today
      t.float :temperature
      t.string :description

      t.timestamps
    end
  end

  def self.down
    drop_table :weathers
  end
end
